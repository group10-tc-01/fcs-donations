using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Domain.Donations;

namespace Fcs.Donations.Application.UseCases.Donations.GetDonations;

[ExcludeFromCodeCoverage]
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
}
