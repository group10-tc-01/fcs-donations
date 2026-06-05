using fcs.Donations.Application.Abstractions.ExternalServices;

namespace fcs.Donations.CommomTestsUtilities.TestDoubles;

public sealed class FakeCampaignEligibilityClient : ICampaignEligibilityClient
{
    public bool IsEligible { get; set; } = true;

    public Task<CampaignEligibilityResponse> CheckEligibilityAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CampaignEligibilityResponse(IsEligible, IsEligible ? null : "Campaign is not eligible."));
    }
}
