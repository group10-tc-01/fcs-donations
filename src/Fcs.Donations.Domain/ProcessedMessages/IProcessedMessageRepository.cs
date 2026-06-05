namespace Fcs.Donations.Domain.ProcessedMessages;

public interface IProcessedMessageRepository
{
    Task<bool> ExistsByMessageIdAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task AddAsync(ProcessedMessage message, CancellationToken cancellationToken = default);
}
