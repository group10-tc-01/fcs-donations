using Fcs.Donations.Domain.OutboxMessages;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

public sealed class InMemoryOutboxMessageRepository : IOutboxMessageRepository
{
    private readonly List<OutboxMessage> _messages = new();

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var pending = _messages
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToList();

        return Task.FromResult((IReadOnlyList<OutboxMessage>)pending);
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        var index = _messages.FindIndex(m => m.Id == message.Id);
        if (index >= 0)
        {
            _messages[index] = message;
        }
        return Task.CompletedTask;
    }
}
