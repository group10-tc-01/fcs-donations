using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.Audit;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.OutboxMessages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Donations.Infrastructure.Kafka.Messaging;

[ExcludeFromCodeCoverage]
public sealed class OutboxMessageProcessor
{
    private readonly IOutboxMessageRepository _repository;
    private readonly IMessagePublisher _publisher;
    private readonly KafkaTopicsSettings _kafkaTopics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OutboxMessageProcessor> _logger;

    public OutboxMessageProcessor(
        IOutboxMessageRepository repository,
        IMessagePublisher publisher,
        IUnitOfWork unitOfWork,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<OutboxMessageProcessor> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _kafkaTopics = kafkaSettings.Value.Topics;
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
            await _publisher.PublishAsync("donation-received", message.Payload, cancellationToken);
            message.MarkPublished();
            await _repository.UpdateAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _publisher.PublishAuditLogFireAndForget(_kafkaTopics.AuditLog, CreatePublishedAudit(message));
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
