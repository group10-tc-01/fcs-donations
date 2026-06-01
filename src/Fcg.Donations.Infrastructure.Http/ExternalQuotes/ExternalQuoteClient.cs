using Fcg.Donations.Application.Abstractions.ExternalServices;

namespace Fcg.Donations.Infrastructure.Http.ExternalQuotes;

public sealed class ExternalQuoteClient : IExternalQuoteClient
{
    private readonly IGitHubZenApi _api;

    public ExternalQuoteClient(IGitHubZenApi api)
    {
        _api = api;
    }

    public Task<string> GetZenAsync(CancellationToken cancellationToken) =>
        _api.GetZenAsync(cancellationToken);
}
