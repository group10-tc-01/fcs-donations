using Refit;

namespace Fcg.Donations.Infrastructure.Http.CampaignEligibility;

public sealed record DonationEligibilityResponse(bool IsEligible, string? Reason);

public interface ICampaignEligibilityApi
{
    [Get("/internal/campaigns/{campaignId}/donation-eligibility")]
    Task<DonationEligibilityResponse> CheckEligibilityAsync(Guid campaignId, CancellationToken cancellationToken);
}
