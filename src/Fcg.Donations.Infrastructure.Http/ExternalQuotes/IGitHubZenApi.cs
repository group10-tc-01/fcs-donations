using Refit;

namespace Fcg.Donations.Infrastructure.Http.ExternalQuotes;

public interface IGitHubZenApi
{
    [Get("/zen")]
    Task<string> GetZenAsync(CancellationToken cancellationToken);
}
