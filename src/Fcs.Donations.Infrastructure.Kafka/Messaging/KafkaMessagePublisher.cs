using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Fcs.Donations.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Donations.Infrastructure.Kafka.Messaging;

public sealed class KafkaMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaMessagePublisher> _logger;

    public KafkaMessagePublisher(IOptions<KafkaSettings> options, ILogger<KafkaMessagePublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<TMessage>(string topicName, TMessage message, CancellationToken cancellationToken = default)
    {
        var payload = message is string rawPayload ? rawPayload : JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(topicName, CreateMessage(payload), cancellationToken);
        _logger.LogInformation("Published message to topic {TopicName}", topicName);
    }

    private static Message<Null, string> CreateMessage(string payload)
    {
        var headers = new Headers();
        var activity = Activity.Current;

        if (activity?.Id is { } traceParent)
        {
            headers.Add("traceparent", Encoding.UTF8.GetBytes(traceParent));
        }

        if (!string.IsNullOrWhiteSpace(activity?.TraceStateString))
        {
            headers.Add("tracestate", Encoding.UTF8.GetBytes(activity.TraceStateString));
        }

        return new Message<Null, string> { Value = payload, Headers = headers };
    }

    public void Dispose() => _producer.Dispose();
}
