using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Messages;

[ExcludeFromCodeCoverage]
public static class ResourceMessages
{
    public const string DonationInvalidAmountCode = "Donation.InvalidAmount";
    public const string DonationInvalidCampaignIdCode = "Donation.InvalidCampaignId";
    public const string DonationUnauthenticatedCode = "Donation.Unauthenticated";
    public const string DonationNotFoundCode = "Donation.NotFound";
    public const string DonationCampaignNotEligibleCode = "Donation.CampaignNotEligible";
    public const string CampaignServiceUnavailableCode = "Campaign.ServiceUnavailable";
    public const string CampaignRequestRejectedCode = "Campaign.RequestRejected";
    public const string CampaignNotFoundCode = "Campaign.NotFound";


    public const string DonationAmountInvalid = "Donation amount must be greater than zero.";
    public const string CampaignNotEligible = "Campaign is not eligible to receive donations.";
    public const string DonationNotFound = "Donation not found.";
    public const string DonationUnauthenticated = "User must be authenticated.";
    public const string CampaignServiceUnavailable = "Campaign service is temporarily unavailable.";
    public const string CampaignRequestRejected = "Campaign eligibility request was rejected.";
    public const string CampaignWasNotFound = "Campaign was not found.";
}
