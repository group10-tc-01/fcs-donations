using fcs.Donations.Domain.Abstractions;
using fcs.Donations.Domain.Donations;
using fcs.Donations.Infrastructure.MongoDb.HealthChecks;
using fcs.Donations.Infrastructure.MongoDb.Persistence;
using fcs.Donations.Infrastructure.MongoDb.Persistence.Repositories;
using fcs.Donations.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace fcs.Donations.Infrastructure.MongoDb.DependencyInjection;

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
