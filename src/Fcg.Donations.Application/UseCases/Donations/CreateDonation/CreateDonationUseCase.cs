using Fcg.Donations.Application.Abstractions.Authentication;
using Fcg.Donations.Application.Abstractions.ExternalServices;
using Fcg.Donations.Application.Abstractions.Messaging;
using Fcg.Donations.Domain;
using Fcg.Donations.Domain.Abstractions;
using Fcg.Donations.Domain.Donations;
using Fcg.Donations.Domain.OutboxMessages;
using Fcg.Donations.Messages;
using System.Text.Json;

namespace Fcg.Donations.Application.UseCases.Donations.CreateDonation;

public sealed class CreateDonationUseCase : ICreateDonationUseCase
{
    private readonly IDonationRepository _donationRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICampaignEligibilityClient _campaignClient;
    private readonly ILoggedUserService _loggedUser;

    public CreateDonationUseCase(
        IDonationRepository donationRepository,
        IOutboxMessageRepository outboxMessageRepository,
        IUnitOfWork unitOfWork,
        ICampaignEligibilityClient campaignClient,
        ILoggedUserService loggedUser)
    {
        _donationRepository = donationRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _unitOfWork = unitOfWork;
        _campaignClient = campaignClient;
        _loggedUser = loggedUser;
    }

    public async Task<Result<CreateDonationResponse>> Handle(CreateDonationRequest request, CancellationToken cancellationToken)
    {
        var donorId = _loggedUser.GetUserId();

        if (donorId is null)
        {
            return Error.Failure("Donation.Unauthenticated", "User must be authenticated.");
        }

        var eligibility = await _campaignClient.CheckEligibilityAsync(request.CampaignId, cancellationToken);

        if (!eligibility.IsEligible)
        {
            return Error.Conflict("Donation.CampaignNotEligible", eligibility.Reason ?? ResourceMessages.CampaignNotEligible);
        }

        var donationResult = Donation.Create(request.CampaignId, donorId.Value, request.Amount);

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
