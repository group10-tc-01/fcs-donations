using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class FakeMessagePublisher : IMessagePublisher
{
    public List<object> PublishedMessages { get; } = new();

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        PublishedMessages.Add(message!);
        return Task.CompletedTask;
    }
}
