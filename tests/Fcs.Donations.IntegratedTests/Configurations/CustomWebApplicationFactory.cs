using fcs.Donations.Application.Abstractions.Authentication;
using fcs.Donations.Application.Abstractions.ExternalServices;
using fcs.Donations.CommomTestsUtilities.TestDoubles;
using fcs.Donations.Domain.Abstractions;
using fcs.Donations.Domain.Donations;
using fcs.Donations.Domain.OutboxMessages;
using fcs.Donations.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace fcs.Donations.IntegratedTests.Configurations;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryDonationRepository DonationRepository { get; } = new();
    public InMemoryOutboxMessageRepository OutboxRepository { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FakeCampaignEligibilityClient CampaignClient { get; } = new();
    public FakeLoggedUserService LoggedUser { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDonationRepository>();
            services.RemoveAll<IOutboxMessageRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.RemoveAll<ICampaignEligibilityClient>();
            services.RemoveAll<ILoggedUserService>();

            services.AddSingleton<IDonationRepository>(DonationRepository);
            services.AddSingleton<IOutboxMessageRepository>(OutboxRepository);
            services.AddSingleton<IUnitOfWork>(UnitOfWork);
            services.AddSingleton<ICampaignEligibilityClient>(CampaignClient);
            services.AddSingleton<ILoggedUserService>(LoggedUser);
        });
    }
}
