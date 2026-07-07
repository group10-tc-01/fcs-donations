namespace Fcs.Donations.Application.Audit;

public interface IAuditPublisher
{
    Task PublishAsync(AuditLogRequestedEvent auditEvent, CancellationToken cancellationToken);
}
