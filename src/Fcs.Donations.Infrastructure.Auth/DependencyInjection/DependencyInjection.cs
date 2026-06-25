using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Settings;
using Fcs.Donations.Infrastructure.Auth.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

namespace Fcs.Donations.Infrastructure.Auth.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = configuration["Keycloak:Authority"];
            options.Audience = configuration["Keycloak:Audience"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateLifetime = true,
                NameClaimType = "preferred_username",
                RoleClaimType = ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity is not ClaimsIdentity identity)
                    {
                        return Task.CompletedTask;
                    }

                    var realmAccess = context.Principal.FindFirstValue("realm_access");
                    if (string.IsNullOrWhiteSpace(realmAccess))
                    {
                        return Task.CompletedTask;
                    }

                    using var document = JsonDocument.Parse(realmAccess);
                    if (!document.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
                    {
                        return Task.CompletedTask;
                    }

                    foreach (var role in roles.EnumerateArray())
                    {
                        if (role.ValueKind == JsonValueKind.String)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddScoped<IPasswordEncrypterService, PasswordEncrypterService>();
        services.AddScoped<ITokenProviderService, TokenProviderService>();
        services.AddScoped<ILoggedUserService, LoggedUserService>();

        return services;
    }
}
