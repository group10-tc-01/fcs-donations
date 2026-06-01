using Fcg.Donations.Application.Abstractions.Authentication;
using Fcg.Donations.Application.Abstractions.ExternalServices;
using Fcg.Donations.CommomTestsUtilities.TestDoubles;
using Fcg.Donations.Domain.Abstractions;
using Fcg.Donations.Domain.Donations;
using Fcg.Donations.Domain.OutboxMessages;
using Fcg.Donations.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fcg.Donations.IntegratedTests.Configurations;

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
