namespace fcs.Donations.Messages;

public sealed record DonationReceivedEvent(
    Guid EventId,
    Guid DonationId,
    Guid CampaignId,
    Guid DonorId,
    decimal Amount,
    DateTime OccurredAt);
