using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.Messaging;

public sealed class NullMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
