using Fcs.Donations.Domain.ProcessedMessages;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;

public sealed class ProcessedMessageRepository : IProcessedMessageRepository
{
    private readonly FcsDonationsDbContext _dbContext;

    public ProcessedMessageRepository(FcsDonationsDbContext dbContext)
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
