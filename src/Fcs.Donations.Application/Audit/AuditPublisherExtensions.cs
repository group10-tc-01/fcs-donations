using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.Audit;

[ExcludeFromCodeCoverage]
public static class AuditPublisherExtensions
{
    public static void PublishAuditLogFireAndForget(
        this IMessagePublisher messagePublisher,
        string topicName,
        AuditLogRequestedEvent auditEvent)
    {
        var publishTask = messagePublisher.PublishAsync(topicName, auditEvent, CancellationToken.None);
        if (publishTask.IsCompletedSuccessfully)
        {
            return;
        }

        _ = publishTask.ContinueWith(
            task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }
}
