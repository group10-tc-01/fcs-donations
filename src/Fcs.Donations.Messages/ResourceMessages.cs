namespace Fcs.Donations.Messages;

public static class ResourceMessages
{
    public const string ItemAlreadyExistsCode = "Item.AlreadyExists";
    public const string ItemNotFoundCode = "Item.NotFound";
    public const string ItemNameRequiredCode = "Item.NameRequired";
    public const string ItemInvalidPriceCode = "Item.InvalidPrice";
    public const string DonationInvalidAmountCode = "Donation.InvalidAmount";
    public const string DonationInvalidCampaignIdCode = "Donation.InvalidCampaignId";
    public const string DonationUnauthenticatedCode = "Donation.Unauthenticated";
    public const string DonationCampaignNotEligibleCode = "Donation.CampaignNotEligible";

    public const string ItemAlreadyExists = "An item with the same name already exists.";
    public const string ItemNotFound = "Item not found.";
    public const string DonationAmountInvalid = "Donation amount must be greater than zero.";
    public const string CampaignNotEligible = "Campaign is not eligible to receive donations.";
    public const string DonationNotFound = "Donation not found.";
    public const string ItemNameIsRequired = "item name is required.";
    public const string ItemPriceIsRequired = "item price is required.";
    public const string DonationUnauthenticated = "User must be authenticated.";
}
