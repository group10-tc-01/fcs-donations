namespace fcs.Donations.Application.Abstractions.ExternalServices;

public sealed record CampaignEligibilityResponse(bool IsEligible, string? Reason);

public interface ICampaignEligibilityClient
{
    Task<CampaignEligibilityResponse> CheckEligibilityAsync(Guid campaignId, CancellationToken cancellationToken);
}
