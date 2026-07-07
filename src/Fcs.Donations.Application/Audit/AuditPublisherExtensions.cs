using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Application.Audit;

[ExcludeFromCodeCoverage]
public static class AuditPublisherExtensions
{
    public static void PublishAuditLogFireAndForget(this IAuditPublisher auditPublisher, AuditLogRequestedEvent auditEvent)
    {
        var publishTask = auditPublisher.PublishAsync(auditEvent, CancellationToken.None);
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
