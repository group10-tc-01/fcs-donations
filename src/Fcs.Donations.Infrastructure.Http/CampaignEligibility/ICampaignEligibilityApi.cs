using Refit;

namespace Fcs.Donations.Infrastructure.Http.CampaignEligibility;

public sealed record DonationEligibilityResponse(Guid CampaignId, bool Eligible, string? Reason);

public sealed record CampaignEligibilityApiResponse(
    bool Success,
    DonationEligibilityResponse? Data,
    string? Message);

public interface ICampaignEligibilityApi
{
    [Get("/api/v1/internal/campaigns/{campaignId}/donation-eligibility")]
    Task<CampaignEligibilityApiResponse> CheckEligibilityAsync(
        Guid campaignId,
        CancellationToken cancellationToken);
}
