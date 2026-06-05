namespace fcs.Donations.WebApi.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "fcs.Donations";
}
