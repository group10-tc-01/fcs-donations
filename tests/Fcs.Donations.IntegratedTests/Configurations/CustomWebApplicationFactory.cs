using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.Abstractions.ExternalServices;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Fcs.Donations.IntegratedTests.Configurations;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryDonationRepository DonationRepository { get; } = new();
    public InMemoryOutboxMessageRepository OutboxRepository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FakeCampaignEligibilityClient CampaignClient { get; } = new();
    public FakeLoggedUserService LoggedUser { get; } = new();

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("JwtSettings__SecretKey", AuthTestHelper.SecretKey);
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "Fcs.Donations");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "Fcs.Donations.Client");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

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
    }
}
