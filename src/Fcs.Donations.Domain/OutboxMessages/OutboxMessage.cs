using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Domain.OutboxMessages;

[ExcludeFromCodeCoverage]
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public OutboxMessage(Guid id, Guid aggregateId, string eventType, string payload)
    {
        Id = id;
        AggregateId = aggregateId;
        EventType = eventType;
        Payload = payload;
        Status = OutboxMessageStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public Guid Id { get; private set; }
    public Guid AggregateId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public OutboxMessageStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    public void MarkPublished()
    {
        Status = OutboxMessageStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = OutboxMessageStatus.Failed;
        LastError = error;
        RetryCount++;
    }

    public void IncrementRetry()
    {
        RetryCount++;
    }
}
