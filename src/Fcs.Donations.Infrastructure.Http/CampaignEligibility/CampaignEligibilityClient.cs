using Fcs.Donations.Application.Abstractions.ExternalServices;

namespace Fcs.Donations.Infrastructure.Http.CampaignEligibility;

public sealed class CampaignEligibilityClient : ICampaignEligibilityClient
{
    private readonly ICampaignEligibilityApi _api;

    public CampaignEligibilityClient(ICampaignEligibilityApi api)
    {
        _api = api;
    }

    public async Task<CampaignEligibilityResponse> CheckEligibilityAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var response = await _api.CheckEligibilityAsync(campaignId, cancellationToken);
        return new CampaignEligibilityResponse(response.IsEligible, response.Reason);
    }
}
