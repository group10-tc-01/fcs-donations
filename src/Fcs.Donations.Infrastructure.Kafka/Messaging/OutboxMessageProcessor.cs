using Fcs.Donations.Application.Audit;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.OutboxMessages;
using Microsoft.Extensions.Logging;

namespace Fcs.Donations.Infrastructure.Kafka.Messaging;

public sealed class OutboxMessageProcessor
{
    private readonly IOutboxMessageRepository _repository;
    private readonly IOutboxMessagePublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;
    private readonly ILogger<OutboxMessageProcessor> _logger;

    public OutboxMessageProcessor(
        IOutboxMessageRepository repository,
        IOutboxMessagePublisher publisher,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher,
        ILogger<OutboxMessageProcessor> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
        _logger = logger;
    }

    public async Task ProcessPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pending = await _repository.GetPendingAsync(batchSize, cancellationToken);

        foreach (var message in pending)
        {
            await ProcessAsync(message, cancellationToken);
        }
    }

    private async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await _publisher.PublishRawAsync(message.Payload, cancellationToken);
            message.MarkPublished();
            await _repository.UpdateAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _auditPublisher.PublishAuditLogFireAndForget(CreatePublishedAudit(message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
            message.MarkFailed(ex.Message);
            await _repository.UpdateAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static AuditLogRequestedEvent CreatePublishedAudit(OutboxMessage message)
    {
        return AuditLogRequestedEvent.Create(
            AuditActions.DonationEventPublished,
            nameof(OutboxMessage),
            message.Id.ToString(),
            null,
            "System",
            new Dictionary<string, object?>
            {
                ["donationId"] = message.AggregateId,
                ["eventType"] = message.EventType,
                ["publishedAt"] = message.PublishedAt
            });
    }
}
