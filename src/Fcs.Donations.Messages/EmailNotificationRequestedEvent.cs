namespace Fcs.Donations.Messages;

public sealed record EmailNotificationRequestedEvent(
    Guid EventId,
    string Type,
    string RecipientEmail,
    Guid? DonationId,
    decimal? Amount,
    DateTime OccurredAt)
{
    public const string DonationCreated = "DonationCreated";
}
