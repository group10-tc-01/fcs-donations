using System.Diagnostics.CodeAnalysis;

namespace Fcs.Identity.Infrastructure.Keycloak.Settings;

[ExcludeFromCodeCoverage]
public sealed class KeycloakSettings
{
    public const string SectionName = "Keycloak";

    public string Authority { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;
}
