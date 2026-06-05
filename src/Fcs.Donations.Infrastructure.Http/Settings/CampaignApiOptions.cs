namespace Fcs.Donations.Infrastructure.Http.Settings;

public sealed class CampaignApiOptions
{
    public const string SectionName = "CampaignApi";

    public string BaseUrl { get; init; } = "http://localhost:5001";
    public int TimeoutSeconds { get; init; } = 10;
    public CampaignApiRetryOptions Retry { get; init; } = new();
}
