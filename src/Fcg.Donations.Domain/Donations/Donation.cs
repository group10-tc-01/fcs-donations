namespace Fcg.Donations.Domain.Donations;

public sealed class Donation
{
    private Donation()
    {
    }

    private Donation(Guid id, Guid campaignId, Guid donorId, decimal amount)
    {
        Id = id;
        CampaignId = campaignId;
        DonorId = donorId;
        Amount = amount;
        Status = DonationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid DonorId { get; private set; }
    public decimal Amount { get; private set; }
    public DonationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    public static Result<Donation> Create(Guid campaignId, Guid donorId, decimal amount)
    {
        if (amount <= 0)
        {
            return Error.Validation("Donation.InvalidAmount", "Donation amount must be greater than zero.");
        }

        return new Donation(Guid.NewGuid(), campaignId, donorId, amount);
    }

    public void MarkProcessed()
    {
        Status = DonationStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = DonationStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        FailureReason = reason;
    }
}
