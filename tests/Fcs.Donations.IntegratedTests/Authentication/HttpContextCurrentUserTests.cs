using System.Security.Claims;
using System.Text.Json;
using Fcs.Donations.Infrastructure.Auth.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Fcs.Donations.IntegratedTests.Authentication;

public sealed class HttpContextCurrentUserTests
{
    [Fact]
    public void Given_AuthenticatedUser_When_ReadProperties_Then_ShouldReturnIdentityData()
    {
        var userId = Guid.NewGuid().ToString();
        var realmAccess = JsonSerializer.Serialize(new { roles = new[] { "Doador", "GestorONG" } });
        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim(ClaimTypes.Role, "Doador"),
            new Claim("roles", "GestorONG"),
            new Claim("realm_access", realmAccess)
        };
        var sut = CreateCurrentUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")));

        sut.IsAuthenticated.Should().BeTrue();
        sut.KeycloakUserId.Should().Be(userId);
        sut.Roles.Should().Contain(["Doador", "GestorONG"]);
    }

    [Fact]
    public void Given_NameIdentifierClaim_When_ReadKeycloakUserId_Then_ShouldReturnFallbackIdentifier()
    {
        var userId = Guid.NewGuid().ToString();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var sut = CreateCurrentUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")));

        sut.KeycloakUserId.Should().Be(userId);
    }

    [Fact]
    public void Given_AnonymousUser_When_ReadProperties_Then_ShouldReturnEmptyValues()
    {
        var sut = CreateCurrentUser(new ClaimsPrincipal(new ClaimsIdentity()));

        sut.IsAuthenticated.Should().BeFalse();
        sut.KeycloakUserId.Should().BeNull();
        sut.Roles.Should().BeEmpty();
    }

    [Fact]
    public void Given_RealmAccessWithoutRoles_When_ReadRoles_Then_ShouldIgnoreRealmAccess()
    {
        var claims = new[] { new Claim("realm_access", "{}") };
        var sut = CreateCurrentUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")));

        sut.Roles.Should().BeEmpty();
    }

    private static HttpContextCurrentUser CreateCurrentUser(ClaimsPrincipal principal)
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return new HttpContextCurrentUser(httpContextAccessor);
    }
}
