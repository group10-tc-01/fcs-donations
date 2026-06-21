namespace Fcs.Donations.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? KeycloakUserId { get; }
    IReadOnlyCollection<string> Roles { get; }
}
