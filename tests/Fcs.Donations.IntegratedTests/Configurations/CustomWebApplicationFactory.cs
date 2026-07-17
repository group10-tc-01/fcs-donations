using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.ExternalServices;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fcs.Donations.IntegratedTests.Configurations;

[ExcludeFromCodeCoverage]
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AuthenticationScheme = "Test";

    public InMemoryDonationRepository DonationRepository { get; } = new();
    public InMemoryOutboxMessageRepository OutboxRepository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FakeCampaignEligibilityClient CampaignClient { get; } = new();
    public FakeCurrentUser CurrentUser { get; } = new();
    public FakeMessagePublisher MessagePublisher { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SqlServer"] =
                    "Server=invalid-donations-test-host;Database=DonationsDb;User Id=sa;Password=Invalid123!;TrustServerCertificate=True;Connect Timeout=1;"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = null;
                options.MetadataAddress = string.Empty;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthTestHelper.SecretKey)),
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };
            });

            services.RemoveAll<IDonationRepository>();
            services.RemoveAll<IOutboxMessageRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<ICampaignEligibilityClient>();
            services.RemoveAll<ICurrentUser>();
            services.RemoveAll<IMessagePublisher>();

            services.AddSingleton<IDonationRepository>(DonationRepository);
            services.AddSingleton<IOutboxMessageRepository>(OutboxRepository);
            services.AddSingleton<IUnitOfWork>(UnitOfWork);
            services.AddSingleton<ICampaignEligibilityClient>(CampaignClient);
            services.AddSingleton<ICurrentUser>(CurrentUser);
            services.AddSingleton<IMessagePublisher>(MessagePublisher);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthenticationScheme;
                options.DefaultChallengeScheme = AuthenticationScheme;
                options.DefaultForbidScheme = AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(AuthenticationScheme, _ => { });
        });
    }

    private sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ICurrentUser _currentUser;

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ICurrentUser currentUser) : base(options, logger, encoder)
        {
            _currentUser = currentUser;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, _currentUser.KeycloakUserId ?? string.Empty),
                new(ClaimTypes.Email, _currentUser.Email ?? "doador@teste.local")
            };

            claims.AddRange(_currentUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
