using Fcg.Donations.Domain.Abstractions;
using Fcg.Donations.Domain.Donations;
using Fcg.Donations.Domain.Items;
using Fcg.Donations.Domain.OutboxMessages;
using Fcg.Donations.Domain.ProcessedMessages;
using Fcg.Donations.Infrastructure.SqlServer.Persistence;
using Fcg.Donations.Infrastructure.SqlServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Donations.Infrastructure.SqlServer.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CleanApiDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CleanApiDbContext>());
        services.AddHealthChecks().AddDbContextCheck<CleanApiDbContext>("sqlserver");

        return services;
    }
}
