using System.Net;
using System.Text.Json;
using Fcs.Donations.Application.Abstractions.ExternalServices;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Messages;
using Microsoft.Extensions.Logging;
using Refit;

namespace Fcs.Donations.Infrastructure.Http.CampaignEligibility;

public sealed class CampaignEligibilityClient : ICampaignEligibilityClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ICampaignEligibilityApi _api;
    private readonly ILogger<CampaignEligibilityClient> _logger;

    public CampaignEligibilityClient(
        ICampaignEligibilityApi api,
        ILogger<CampaignEligibilityClient> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<Result<CampaignEligibilityResponse>> CheckEligibilityAsync(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _api.CheckEligibilityAsync(campaignId, cancellationToken);

            if (!response.Success)
            {
                return Error.Validation(
                    ResourceMessages.CampaignRequestRejectedCode,
                    response.Message ?? ResourceMessages.CampaignRequestRejected);
            }

            if (response.Data is null)
            {
                _logger.LogError(
                    "Campaign eligibility response did not contain data for campaign {CampaignId}.",
                    campaignId);

                return ServiceUnavailable();
            }

            return new CampaignEligibilityResponse(response.Data.Eligible, response.Data.Reason);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogWarning(
                exception,
                "Campaign eligibility request timed out for campaign {CampaignId}.",
                campaignId);

            return ServiceUnavailable();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Campaign eligibility request failed for campaign {CampaignId}.",
                campaignId);

            return ServiceUnavailable();
        }
        catch (ApiException exception)
        {
            return HandleApiException(campaignId, exception);
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Campaign eligibility response was invalid for campaign {CampaignId}.",
                campaignId);

            return ServiceUnavailable();
        }
    }

    private Result<CampaignEligibilityResponse> HandleApiException(
        Guid campaignId,
        ApiException exception)
    {
        var message = ReadErrorMessage(exception.Content);

        _logger.LogWarning(
            exception,
            "Campaign eligibility request returned status {StatusCode} for campaign {CampaignId}.",
            (int)exception.StatusCode,
            campaignId);

        return exception.StatusCode switch
        {
            HttpStatusCode.BadRequest => Error.Validation(
                ResourceMessages.CampaignRequestRejectedCode,
                message ?? ResourceMessages.CampaignRequestRejected),
            HttpStatusCode.NotFound => Error.NotFound(
                ResourceMessages.CampaignNotFoundCode,
                message ?? ResourceMessages.CampaignWasNotFound),
            HttpStatusCode.Conflict => Error.Validation(
                ResourceMessages.DonationCampaignNotEligibleCode,
                message ?? ResourceMessages.CampaignNotEligible),
            HttpStatusCode.RequestTimeout => ServiceUnavailable(),
            >= HttpStatusCode.InternalServerError => ServiceUnavailable(),
            _ => ServiceUnavailable()
        };
    }

    private static string? ReadErrorMessage(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CampaignEligibilityApiResponse>(content, JsonOptions)?.Message;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static Error ServiceUnavailable() =>
        Error.ServiceUnavailable(
            ResourceMessages.CampaignServiceUnavailableCode,
            ResourceMessages.CampaignServiceUnavailable);
}
