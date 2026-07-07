using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.Audit;
using Fcs.Donations.Infrastructure.Kafka.Messaging;
using Fcs.Donations.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcs.Donations.Infrastructure.Kafka.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddKafkaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetSection(KafkaSettings.SectionName));
        services.AddSingleton<KafkaMessagePublisher>();
        services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<KafkaMessagePublisher>());
        services.AddSingleton<IOutboxMessagePublisher>(sp => sp.GetRequiredService<KafkaMessagePublisher>());
        services.AddSingleton<IAuditPublisher, KafkaAuditPublisher>();
        services.AddScoped<OutboxMessageProcessor>();
        services.AddHostedService<OutboxPublisher>();
        return services;
    }
}
