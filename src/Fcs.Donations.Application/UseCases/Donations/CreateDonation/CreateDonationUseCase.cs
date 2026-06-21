using System.Text.Json;
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

    public CreateDonationUseCase(
        IDonationRepository donationRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IUnitOfWork unitOfWork,
        ICampaignEligibilityClient campaignClient,
        ICurrentUser currentUser)
    {
        _donationRepository = donationRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
        _campaignClient = campaignClient;
        _currentUser = currentUser;
    }

    public async Task<Result<CreateDonationResponse>> Handle(CreateDonationRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            !Guid.TryParse(_currentUser.KeycloakUserId, out var donorId))
        {
            return Error.Failure(ResourceMessages.DonationUnauthenticatedCode, ResourceMessages.DonationUnauthenticated);
        }

        var eligibility = await _campaignClient.CheckEligibilityAsync(request.CampaignId, cancellationToken);

        if (!eligibility.IsEligible)
        {
            return Error.Conflict(
                ResourceMessages.DonationCampaignNotEligibleCode,
                eligibility.Reason ?? ResourceMessages.CampaignNotEligible);
        }

        var donationResult = Donation.Create(request.CampaignId, donorId, request.Amount);

        if (donationResult.IsFailure)
        {
            return donationResult.Error;
        }

        var donation = donationResult.Value;

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

        return new CreateDonationResponse(donation.Id, donation.CampaignId, donation.Amount, donation.CreatedAt);
    }
}
