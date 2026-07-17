using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class FakeMessagePublisher : IMessagePublisher
{
    public List<object> PublishedMessages { get; } = new();
    public List<string> Topics { get; } = new();
    public Exception? ExceptionToThrow { get; set; }
    public string? ExceptionTopicName { get; set; }

    public Task PublishAsync<TMessage>(string topicName, TMessage message, CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow is not null &&
            (ExceptionTopicName is null || ExceptionTopicName == topicName))
        {
            throw ExceptionToThrow;
        }

        Topics.Add(topicName);
        PublishedMessages.Add(message!);
        return Task.CompletedTask;
    }
}
