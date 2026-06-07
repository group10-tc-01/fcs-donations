using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.WebApi.Observability;

[ExcludeFromCodeCoverage]
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "Fcs.Donations";
}
