using System.Text.Json;
using Fcs.Donations.Application.Audit;
using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.ExternalServices;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Messages;

namespace Fcs.Donations.Application.UseCases.Donations.CreateDonation;

public sealed class CreateDonationUseCase : ICreateDonationUseCase
{
    private readonly IDonationRepository _donationRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICampaignEligibilityClient _campaignClient;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditPublisher? _auditPublisher;

    public CreateDonationUseCase(
        IDonationRepository donationRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IUnitOfWork unitOfWork,
        ICampaignEligibilityClient campaignClient,
        ICurrentUser currentUser,
        IAuditPublisher? auditPublisher = null)
    {
        _donationRepository = donationRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
        _campaignClient = campaignClient;
        _currentUser = currentUser;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<CreateDonationResponse>> Handle(CreateDonationRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !Guid.TryParse(_currentUser.KeycloakUserId, out var donorId))
        {
            PublishRejectedAudit(null, null, "Public", request, ResourceMessages.DonationUnauthenticated);
            return Error.Failure(ResourceMessages.DonationUnauthenticatedCode, ResourceMessages.DonationUnauthenticated);
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
            donation.CreatedAt);

        var payload = JsonSerializer.Serialize(donationEvent);
        var outboxMessage = new OutboxMessage(eventId, donation.Id, nameof(DonationReceivedEvent), payload);

        await _donationRepository.AddAsync(donation, cancellationToken);
        await _outboxMessageRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        PublishEventQueuedAudit(outboxMessage, donation);

        return new CreateDonationResponse(donation.Id, donation.CampaignId, donation.Amount, donation.CreatedAt);
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
        _auditPublisher?.PublishAuditLogFireAndForget(auditEvent);
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
