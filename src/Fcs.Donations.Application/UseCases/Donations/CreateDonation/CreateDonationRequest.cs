using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.UseCases.Donations.CreateDonation;

public sealed record CreateDonationRequest(Guid CampaignId, decimal Amount) : ICommand<CreateDonationResponse>;
