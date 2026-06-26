using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.ExternalServices;
using Fcs.Donations.Domain.Results;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class FakeCampaignEligibilityClient : ICampaignEligibilityClient
{
    public bool IsEligible { get; set; } = true;
    public Error? Error { get; set; }

    public Task<Result<CampaignEligibilityResponse>> CheckEligibilityAsync(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var result = Error is null
            ? Result<CampaignEligibilityResponse>.Success(new CampaignEligibilityResponse(
                IsEligible,
                IsEligible ? null : "Campaign is not eligible."))
            : Result<CampaignEligibilityResponse>.Failure(Error);

        return Task.FromResult(result);
    }
}
