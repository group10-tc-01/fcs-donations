using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class DonationQueryService : IDonationQueryService
{
    private readonly FcsDonationsDbContext _dbContext;

    public DonationQueryService(FcsDonationsDbContext dbContext)
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
