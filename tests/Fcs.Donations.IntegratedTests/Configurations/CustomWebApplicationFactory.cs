using System.Security.Claims;
using System.Text.Encodings.Web;
using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.ExternalServices;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fcs.Donations.IntegratedTests.Configurations;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AuthenticationScheme = "Test";

    public InMemoryDonationRepository DonationRepository { get; } = new();
    public InMemoryOutboxMessageRepository OutboxRepository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FakeCampaignEligibilityClient CampaignClient { get; } = new();
    public FakeLoggedUserService LoggedUser { get; } = new();

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
            services.RemoveAll<IDonationRepository>();
            services.RemoveAll<IDonationQueryService>();
            services.RemoveAll<IOutboxMessageRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<ICampaignEligibilityClient>();
            services.RemoveAll<ILoggedUserService>();

            services.AddSingleton<IDonationRepository>(DonationRepository);
            services.AddSingleton<IDonationQueryService>(new InMemoryDonationQueryService(DonationRepository));
            services.AddSingleton<IOutboxMessageRepository>(OutboxRepository);
            services.AddSingleton<IUnitOfWork>(UnitOfWork);
            services.AddSingleton<ICampaignEligibilityClient>(CampaignClient);
            services.AddSingleton<ILoggedUserService>(LoggedUser);
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
        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Doador")
            };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
