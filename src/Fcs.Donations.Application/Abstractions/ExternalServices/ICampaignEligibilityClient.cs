using Fcs.Donations.Domain.Results;

namespace Fcs.Donations.Application.Abstractions.ExternalServices;

public sealed record CampaignEligibilityResponse(bool IsEligible, string? Reason);

public interface ICampaignEligibilityClient
{
    Task<Result<CampaignEligibilityResponse>> CheckEligibilityAsync(
        Guid campaignId,
        CancellationToken cancellationToken);
}
