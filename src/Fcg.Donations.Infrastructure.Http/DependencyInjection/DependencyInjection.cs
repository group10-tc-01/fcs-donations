using Fcg.Donations.Application.Abstractions.ExternalServices;
using Fcg.Donations.Infrastructure.Http.CampaignEligibility;
using Fcg.Donations.Infrastructure.Http.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace Fcg.Donations.Infrastructure.Http.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddHttpInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CampaignApiOptions>(configuration.GetSection(CampaignApiOptions.SectionName));

        services.AddRefitClient<ICampaignEligibilityApi>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<CampaignApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler((serviceProvider, _) =>
            {
                var retry = serviceProvider.GetRequiredService<IOptions<CampaignApiOptions>>().Value.Retry;
                return CreateRetryPolicy(retry);
            });

        services.AddScoped<ICampaignEligibilityClient, CampaignEligibilityClient>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(CampaignApiRetryOptions retry)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retry.Attempts,
                attempt => TimeSpan.FromMilliseconds(retry.BaseDelayMilliseconds * attempt));
    }
}
