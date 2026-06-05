using Fcs.Donations.Domain.Donations;
using MongoDB.Driver;

namespace Fcs.Donations.Infrastructure.MongoDb.Persistence.Repositories;

public sealed class DonationRepository : IDonationRepository
{
    private readonly MongoDbContext _context;

    public DonationRepository(MongoDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Donation donation, CancellationToken cancellationToken = default)
    {
        return _context.Donations.InsertOneAsync(donation, cancellationToken: cancellationToken);
    }

    public async Task<Donation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Donations.Find(d => d.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(Donation donation, CancellationToken cancellationToken = default)
    {
        await _context.Donations.ReplaceOneAsync(d => d.Id == donation.Id, donation, cancellationToken: cancellationToken);
    }
}
