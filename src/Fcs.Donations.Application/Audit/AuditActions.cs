using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Application.Audit;

[ExcludeFromCodeCoverage]
public static class AuditActions
{
    public const string DonationRequested = nameof(DonationRequested);
    public const string DonationRejected = nameof(DonationRejected);
    public const string DonationEventQueued = nameof(DonationEventQueued);
    public const string DonationEventPublished = nameof(DonationEventPublished);
}
