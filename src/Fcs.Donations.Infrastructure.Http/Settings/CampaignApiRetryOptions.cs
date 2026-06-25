using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Infrastructure.Http.Settings;

[ExcludeFromCodeCoverage]
public sealed class CampaignApiRetryOptions
{
    public int Attempts { get; init; } = 3;
    public int BaseDelayMilliseconds { get; init; } = 200;
}
