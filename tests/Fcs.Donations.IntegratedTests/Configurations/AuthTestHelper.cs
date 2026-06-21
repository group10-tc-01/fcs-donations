using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Fcs.Donations.IntegratedTests.Configurations;

public static class AuthTestHelper
{
    public static string SecretKey { get; } = new('t', 40);

    public static string GenerateToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Doador"),
            new Claim(ClaimTypes.Email, "doador@test.com")
        };

        var token = new JwtSecurityToken(
            issuer: "Fcs.Donations",
            audience: "Fcs.Donations.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
