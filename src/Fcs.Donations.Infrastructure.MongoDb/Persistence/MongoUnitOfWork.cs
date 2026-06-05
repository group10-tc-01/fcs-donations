using fcs.Donations.Domain.Abstractions;

namespace fcs.Donations.Infrastructure.MongoDb.Persistence;

public sealed class MongoUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}
