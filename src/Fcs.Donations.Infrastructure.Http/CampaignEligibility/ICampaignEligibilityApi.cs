using Refit;

namespace Fcs.Donations.Infrastructure.Http.CampaignEligibility;

public sealed record DonationEligibilityResponse(bool IsEligible, string? Reason);

public sealed record CampaignEnvelope<T>(bool Success, T? Data, string? Message);

public interface ICampaignEligibilityApi
{
    [Get("/internal/campaigns/{campaignId}/donation-eligibility")]
    Task<ApiResponse<CampaignEnvelope<DonationEligibilityResponse>>> CheckEligibilityAsync(Guid campaignId, CancellationToken cancellationToken);
}
