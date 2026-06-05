using fcs.Donations.Application.Abstractions.Messaging;

namespace fcs.Donations.Application.Messaging;

public sealed class NullMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
