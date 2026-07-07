using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Confluent.Kafka;
using Fcs.Donations.Application.Audit;
using Fcs.Donations.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Donations.Infrastructure.Kafka.Messaging;

[ExcludeFromCodeCoverage]
public sealed class KafkaAuditPublisher : IAuditPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaAuditPublisher> _logger;

    public KafkaAuditPublisher(IOptions<KafkaSettings> options, ILogger<KafkaAuditPublisher> logger)
    {
        _settings = options.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync(AuditLogRequestedEvent auditEvent, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(auditEvent);
        await _producer.ProduceAsync(_settings.AuditTopicName, new Message<Null, string> { Value = payload }, cancellationToken);
        _logger.LogInformation("Published audit event {Action} to topic {TopicName}", auditEvent.Action, _settings.AuditTopicName);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
