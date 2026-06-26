using Fcs.Donations.Domain.Donations;

namespace Fcs.Donations.Application.UseCases.Donations.GetDonations;

public sealed record DonationQueryResponse
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public Guid DonorId { get; init; }
    public decimal Amount { get; init; }
    public DonationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? FailureReason { get; init; }

    public static DonationQueryResponse FromDomain(Donation donation) => new()
    {
        Id = donation.Id,
        CampaignId = donation.CampaignId,
        DonorId = donation.DonorId,
        Amount = donation.Amount,
        Status = donation.Status,
        CreatedAt = donation.CreatedAt,
        ProcessedAt = donation.ProcessedAt,
        FailureReason = donation.FailureReason
    };
}
