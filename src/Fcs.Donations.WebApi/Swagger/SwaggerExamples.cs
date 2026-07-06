using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Messages;

namespace Fcs.Donations.WebApi.Swagger;

[ExcludeFromCodeCoverage]
public static class SwaggerExamples
{
    public static object CreateDonationRequest => new
    {
        campaignId = "2e1c2f5d-6a4b-4e57-8f3d-4f2f9d60b111",
        amount = 100.00m
    };

    public static object CreateDonationAccepted => Success(new
    {
        id = "8b7e6a4f-0d6a-47b0-9f2f-7e8f3a2c9d21",
        campaignId = "2e1c2f5d-6a4b-4e57-8f3d-4f2f9d60b111",
        amount = 100.00m,
        createdAt = "2026-05-18T20:00:00Z"
    });

    public static object DonationSuccess => Success(Donation());

    public static object DonationsPageSuccess => Success(new
    {
        items = new[]
        {
            Donation()
        },
        page = 1,
        pageSize = 10,
        totalCount = 1,
        totalPages = 1
    });

    public static object ValidationError => Failure("Invalid request data.");

    public static object DonationNotFoundError => Failure(ResourceMessages.DonationNotFound);

    public static object CampaignNotFoundError => Failure(ResourceMessages.CampaignWasNotFound);

    public static object CampaignNotEligibleError => Failure(ResourceMessages.CampaignNotEligible);

    public static object CampaignServiceUnavailableError => Failure(ResourceMessages.CampaignServiceUnavailable);

    private static object Donation() => new
    {
        id = "8b7e6a4f-0d6a-47b0-9f2f-7e8f3a2c9d21",
        campaignId = "2e1c2f5d-6a4b-4e57-8f3d-4f2f9d60b111",
        donorId = "7d75e1f5-3d52-45b7-9a0d-f130b3eb1f8d",
        amount = 100.00m,
        status = "Processed",
        createdAt = "2026-05-18T20:00:00Z",
        processedAt = "2026-05-18T20:00:10Z",
        failureReason = (string?)null
    };

    private static object Success(object data) => new
    {
        success = true,
        data,
        message = (string?)null
    };

    private static object Failure(string message) => new
    {
        success = false,
        data = (object?)null,
        message
    };
}
