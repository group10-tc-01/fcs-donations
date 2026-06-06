using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Items;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.Domain.ProcessedMessages;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Infrastructure.SqlServer.Persistence;
using Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcs.Donations.Infrastructure.SqlServer.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CleanApiDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IDonationQueryService, DonationQueryService>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CleanApiDbContext>());
        services.AddHealthChecks().AddDbContextCheck<CleanApiDbContext>("sqlserver");

        return services;
    }
}
