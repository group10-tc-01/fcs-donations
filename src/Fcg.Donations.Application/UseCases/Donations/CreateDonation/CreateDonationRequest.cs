using Fcg.Donations.Application.Abstractions.Messaging;

namespace Fcg.Donations.Application.UseCases.Donations.CreateDonation;

public sealed record CreateDonationRequest(Guid CampaignId, decimal Amount) : ICommand<CreateDonationResponse>;
