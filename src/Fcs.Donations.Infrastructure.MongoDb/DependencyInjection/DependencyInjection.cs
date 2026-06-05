using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Infrastructure.MongoDb.HealthChecks;
using Fcs.Donations.Infrastructure.MongoDb.Persistence;
using Fcs.Donations.Infrastructure.MongoDb.Persistence.Repositories;
using Fcs.Donations.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Fcs.Donations.Infrastructure.MongoDb.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoDbInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
        services.AddHealthChecks().AddCheck<MongoDbHealthCheck>("mongodb");

        return services;
    }
}
