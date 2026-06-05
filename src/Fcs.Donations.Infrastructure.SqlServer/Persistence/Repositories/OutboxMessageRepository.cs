using Fcs.Donations.Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;

public sealed class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly CleanApiDbContext _dbContext;

    public OutboxMessageRepository(CleanApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        return _dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .AsNoTracking()
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.Update(message);
        return Task.CompletedTask;
    }
}
