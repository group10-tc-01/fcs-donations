using Confluent.Kafka;
using fcs.Donations.Application.Abstractions.Messaging;
using fcs.Donations.Infrastructure.Kafka.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace fcs.Donations.Infrastructure.Kafka.Messaging;

public sealed class KafkaMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topicName;
    private readonly ILogger<KafkaMessagePublisher> _logger;

    public KafkaMessagePublisher(IOptions<KafkaSettings> options, ILogger<KafkaMessagePublisher> logger)
    {
        _topicName = options.Value.TopicName;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(_topicName, new Message<Null, string> { Value = payload }, cancellationToken);
        _logger.LogInformation("Published message to topic {TopicName}", _topicName);
    }

    public async Task PublishRawAsync(string payload, CancellationToken cancellationToken = default)
    {
        await _producer.ProduceAsync(_topicName, new Message<Null, string> { Value = payload }, cancellationToken);
        _logger.LogInformation("Published raw message to topic {TopicName}", _topicName);
    }

    public void Dispose() => _producer.Dispose();
}
