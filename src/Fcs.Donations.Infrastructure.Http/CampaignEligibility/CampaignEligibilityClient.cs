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
        var apiResponse = await _api.CheckEligibilityAsync(campaignId, cancellationToken);
        var data = apiResponse.Content?.Data;
        return new CampaignEligibilityResponse(data?.IsEligible ?? false, data?.Reason);
    }
}
