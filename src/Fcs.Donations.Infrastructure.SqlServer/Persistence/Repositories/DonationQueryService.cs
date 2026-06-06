using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;

public sealed class DonationQueryService : IDonationQueryService
{
    private readonly CleanApiDbContext _dbContext;

    public DonationQueryService(CleanApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<DonationQueryResponse> QueryByDonor(Guid donorId)
    {
        return _dbContext.Donations
            .AsNoTracking()
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
