using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.Authentication;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; } = true;
    public string? KeycloakUserId { get; set; } = Guid.NewGuid().ToString();
    public string? Email { get; set; } = "doador@teste.local";
    public IReadOnlyCollection<string> Roles { get; set; } = ["Doador"];
}
