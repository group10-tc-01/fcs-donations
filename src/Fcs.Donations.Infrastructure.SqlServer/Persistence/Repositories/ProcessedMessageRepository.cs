using fcs.Donations.Domain.ProcessedMessages;
using Microsoft.EntityFrameworkCore;

namespace fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;

public sealed class ProcessedMessageRepository : IProcessedMessageRepository
{
    private readonly CleanApiDbContext _dbContext;

    public ProcessedMessageRepository(CleanApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ProcessedMessage message, CancellationToken cancellationToken = default)
    {
        return _dbContext.ProcessedMessages.AddAsync(message, cancellationToken).AsTask();
    }

    public async Task<bool> ExistsByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProcessedMessages.AnyAsync(pm => pm.MessageId == messageId, cancellationToken);
    }
}
