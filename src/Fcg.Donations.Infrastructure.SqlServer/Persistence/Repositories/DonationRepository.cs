using Fcg.Donations.Domain.Donations;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Donations.Infrastructure.SqlServer.Persistence.Repositories;

public sealed class DonationRepository : IDonationRepository
{
    private readonly CleanApiDbContext _dbContext;

    public DonationRepository(CleanApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Donation donation, CancellationToken cancellationToken = default)
    {
        return _dbContext.Donations.AddAsync(donation, cancellationToken).AsTask();
    }

    public Task<Donation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Donations.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public Task UpdateAsync(Donation donation, CancellationToken cancellationToken = default)
    {
        _dbContext.Donations.Update(donation);
        return Task.CompletedTask;
    }
}
