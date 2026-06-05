using fcs.Donations.Application.Abstractions.Messaging;

namespace fcs.Donations.Application.UseCases.Donations.CreateDonation;

public sealed record CreateDonationRequest(Guid CampaignId, decimal Amount) : ICommand<CreateDonationResponse>;
