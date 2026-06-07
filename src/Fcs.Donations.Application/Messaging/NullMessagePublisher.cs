using Fcs.Donations.Application.Abstractions.Messaging;
using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Application.Messaging;

[ExcludeFromCodeCoverage]
public sealed class NullMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
