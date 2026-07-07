namespace Fcs.Donations.Application.Audit;

public static class AuditActions
{
    public const string DonationRequested = nameof(DonationRequested);
    public const string DonationRejected = nameof(DonationRejected);
    public const string DonationEventQueued = nameof(DonationEventQueued);
    public const string DonationEventPublished = nameof(DonationEventPublished);
}
