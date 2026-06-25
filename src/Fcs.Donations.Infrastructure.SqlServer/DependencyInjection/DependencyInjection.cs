using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.Infrastructure.SqlServer.Persistence;
using Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcs.Donations.Infrastructure.SqlServer.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FcsDonationsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FcsDonationsDbContext>());
        services.AddHealthChecks().AddDbContextCheck<FcsDonationsDbContext>("sqlserver");

        return services;
    }
}
