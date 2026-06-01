namespace Fcg.Donations.Application.UseCases.Donations.CreateDonation;

public sealed record CreateDonationResponse(Guid Id, Guid CampaignId, decimal Amount, DateTime CreatedAt);
