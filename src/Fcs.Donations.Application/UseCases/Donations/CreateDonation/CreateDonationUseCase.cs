using System.Text.Json;
using Fcs.Donations.Application.Audit;
using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.ExternalServices;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Donations.Application.UseCases.Donations.CreateDonation;

public sealed class CreateDonationUseCase : ICreateDonationUseCase
{
    private readonly IDonationRepository _donationRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICampaignEligibilityClient _campaignClient;
    private readonly ICurrentUser _currentUser;
    private readonly IMessagePublisher? _messagePublisher;
    private readonly KafkaTopicsSettings _kafkaTopics;
    private readonly ILogger<CreateDonationUseCase>? _logger;

    public CreateDonationUseCase(
        IDonationRepository donationRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IUnitOfWork unitOfWork,
        ICampaignEligibilityClient campaignClient,
        ICurrentUser currentUser,
        IMessagePublisher? messagePublisher = null,
        IOptions<KafkaSettings>? kafkaSettings = null,
        ILogger<CreateDonationUseCase>? logger = null)
    {
        _donationRepository = donationRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
        _campaignClient = campaignClient;
        _currentUser = currentUser;
        _messagePublisher = messagePublisher;
        _kafkaTopics = kafkaSettings?.Value.Topics ?? new KafkaTopicsSettings();
        _logger = logger;
    }

    public async Task<Result<CreateDonationResponse>> Handle(CreateDonationRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !Guid.TryParse(_currentUser.KeycloakUserId, out var donorId))
        {
            PublishRejectedAudit(null, null, "Public", request, ResourceMessages.DonationUnauthenticated);
            return Error.Failure(ResourceMessages.DonationUnauthenticatedCode, ResourceMessages.DonationUnauthenticated);
        }

        if (string.IsNullOrWhiteSpace(_currentUser.Email))
        {
            return Error.Validation("donation.email_missing", "Authenticated donor token must include an email claim.");
        }

        var eligibilityResult = await _campaignClient.CheckEligibilityAsync(request.CampaignId, cancellationToken);

        if (eligibilityResult.IsFailure)
        {
            PublishRejectedAudit(null, donorId, "Doador", request, eligibilityResult.Error.Message);
            return eligibilityResult.Error;
        }

        if (!eligibilityResult.Value.IsEligible)
        {
            PublishRejectedAudit(null, donorId, "Doador", request, eligibilityResult.Value.Reason ?? ResourceMessages.CampaignNotEligible);
            return Error.Validation(
                ResourceMessages.DonationCampaignNotEligibleCode,
                eligibilityResult.Value.Reason ?? ResourceMessages.CampaignNotEligible);
        }

        var donationResult = Donation.Create(request.CampaignId, donorId, request.Amount);

        if (donationResult.IsFailure)
        {
            PublishRejectedAudit(null, donorId, "Doador", request, donationResult.Error.Message);
            return donationResult.Error;
        }

        var donation = donationResult.Value;
        PublishRequestedAudit(donation, request);

        var eventId = Guid.NewGuid();
        var donationEvent = new DonationReceivedEvent(
            eventId,
            donation.Id,
            donation.CampaignId,
            donation.DonorId,
            donation.Amount,
            donation.CreatedAt,
            _currentUser.Email!);

        var payload = JsonSerializer.Serialize(donationEvent);
        var outboxMessage = new OutboxMessage(eventId, donation.Id, nameof(DonationReceivedEvent), payload);

        await _donationRepository.AddAsync(donation, cancellationToken);
        await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        PublishEventQueuedAudit(outboxMessage, donation);
        await PublishDonationCreatedEmailNotificationAsync(donation, _currentUser.Email, cancellationToken);

        return new CreateDonationResponse(donation.Id, donation.CampaignId, donation.Amount, donation.CreatedAt);
    }

    private async Task PublishDonationCreatedEmailNotificationAsync(Donation donation, string recipientEmail, CancellationToken cancellationToken)
    {
        if (_messagePublisher is null)
        {
            return;
        }

        try
        {
            await _messagePublisher.PublishAsync(
                _kafkaTopics.EmailNotification,
                new EmailNotificationRequestedEvent(Guid.NewGuid(), EmailNotificationRequestedEvent.DonationCreated, recipientEmail, donation.Id, donation.Amount, donation.CreatedAt),
                cancellationToken);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // Email-notification dispatch is best effort and must not revert the donation transaction.
            _logger?.LogError(exception, "Failed to publish donation-created email notification for donation {DonationId}", donation.Id);
        }
    }

    private void PublishRequestedAudit(Donation donation, CreateDonationRequest request)
    {
        PublishAudit(AuditLogRequestedEvent.Create(
            AuditActions.DonationRequested,
            nameof(Donation),
            donation.Id.ToString(),
            donation.DonorId,
            "Doador",
            BuildDonationMetadata(request.CampaignId, request.Amount)));
    }

    private void PublishRejectedAudit(Guid? donationId, Guid? actorId, string? actorType, CreateDonationRequest request, string reason)
    {
        var metadata = BuildDonationMetadata(request.CampaignId, request.Amount).ToDictionary(pair => pair.Key, pair => pair.Value);
        metadata["reason"] = reason;

        PublishAudit(AuditLogRequestedEvent.Create(
            AuditActions.DonationRejected,
            nameof(Donation),
            donationId?.ToString(),
            actorId,
            actorType,
            metadata));
    }

    private void PublishEventQueuedAudit(OutboxMessage outboxMessage, Donation donation)
    {
        PublishAudit(AuditLogRequestedEvent.Create(
            AuditActions.DonationEventQueued,
            nameof(OutboxMessage),
            outboxMessage.Id.ToString(),
            donation.DonorId,
            "Doador",
            new Dictionary<string, object?>
            {
                ["donationId"] = donation.Id,
                ["campaignId"] = donation.CampaignId,
                ["amount"] = donation.Amount,
                ["eventType"] = outboxMessage.EventType
            }));
    }

    private void PublishAudit(AuditLogRequestedEvent auditEvent)
    {
        _messagePublisher?.PublishAuditLogFireAndForget(_kafkaTopics.AuditLog, auditEvent);
    }

    private static IReadOnlyDictionary<string, object?> BuildDonationMetadata(Guid campaignId, decimal amount)
    {
        return new Dictionary<string, object?>
        {
            ["campaignId"] = campaignId,
            ["amount"] = amount
        };
    }
}
