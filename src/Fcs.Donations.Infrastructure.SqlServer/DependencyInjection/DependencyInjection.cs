using fcs.Donations.Domain.Abstractions;
using fcs.Donations.Domain.Donations;
using fcs.Donations.Domain.Items;
using fcs.Donations.Domain.OutboxMessages;
using fcs.Donations.Domain.ProcessedMessages;
using fcs.Donations.Infrastructure.SqlServer.Persistence;
using fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace fcs.Donations.Infrastructure.SqlServer.DependencyInjection;

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
