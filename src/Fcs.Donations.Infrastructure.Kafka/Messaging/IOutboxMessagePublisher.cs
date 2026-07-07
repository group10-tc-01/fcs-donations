namespace Fcs.Donations.Infrastructure.Kafka.Messaging;

public interface IOutboxMessagePublisher
{
    Task PublishRawAsync(string payload, CancellationToken cancellationToken = default);
}
