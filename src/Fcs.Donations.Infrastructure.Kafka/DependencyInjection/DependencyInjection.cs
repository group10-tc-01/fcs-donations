using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Infrastructure.Kafka.Messaging;
using Fcs.Donations.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcs.Donations.Infrastructure.Kafka.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddKafkaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.AddSingleton<KafkaMessagePublisher>();
        services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<KafkaMessagePublisher>());
        services.AddHostedService<OutboxPublisher>();
        return services;
    }
}
