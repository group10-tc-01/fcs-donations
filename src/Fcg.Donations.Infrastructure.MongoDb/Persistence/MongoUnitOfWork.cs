using Fcg.Donations.Domain.Abstractions;

namespace Fcg.Donations.Infrastructure.MongoDb.Persistence;

public sealed class MongoUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}
