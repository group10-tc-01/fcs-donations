using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Audit;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class FakeAuditPublisher : IAuditPublisher
{
    public List<AuditLogRequestedEvent> Events { get; } = new();

    public Task PublishAsync(AuditLogRequestedEvent auditEvent, CancellationToken cancellationToken)
    {
        Events.Add(auditEvent);
        return Task.CompletedTask;
    }
}
