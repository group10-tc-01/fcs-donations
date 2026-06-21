using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.WebApi.Settings;

[ExcludeFromCodeCoverage]
public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
