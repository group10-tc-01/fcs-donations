using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Domain.ProcessedMessages;

[ExcludeFromCodeCoverage]
public sealed class ProcessedMessage
{
    private ProcessedMessage()
    {
    }

    public ProcessedMessage(Guid id, Guid messageId, string topic)
    {
        Id = id;
        MessageId = messageId;
        Topic = topic;
        ProcessedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }
}
