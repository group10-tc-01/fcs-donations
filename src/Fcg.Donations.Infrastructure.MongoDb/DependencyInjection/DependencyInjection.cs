using Fcg.Donations.Domain.Abstractions;
using Fcg.Donations.Domain.Items;
using Fcg.Donations.Infrastructure.MongoDb.HealthChecks;
using Fcg.Donations.Infrastructure.MongoDb.Persistence;
using Fcg.Donations.Infrastructure.MongoDb.Persistence.Repositories;
using Fcg.Donations.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Fcg.Donations.Infrastructure.MongoDb.DependencyInjection;

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
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
        services.AddHealthChecks().AddCheck<MongoDbHealthCheck>("mongodb");

        return services;
    }
}
