using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class InMemoryDonationQueryService : IDonationQueryService
{
    private readonly InMemoryDonationRepository _repository;

    public InMemoryDonationQueryService(InMemoryDonationRepository repository)
    {
        _repository = repository;
    }

    public IQueryable<DonationQueryResponse> QueryByDonor(Guid donorId)
    {
        return _repository.Query()
            .Where(donation => donation.DonorId == donorId)
            .Select(donation => new DonationQueryResponse
            {
                Id = donation.Id,
                CampaignId = donation.CampaignId,
                DonorId = donation.DonorId,
                Amount = donation.Amount,
                Status = donation.Status,
                CreatedAt = donation.CreatedAt,
                ProcessedAt = donation.ProcessedAt,
                FailureReason = donation.FailureReason
            });
    }
}
