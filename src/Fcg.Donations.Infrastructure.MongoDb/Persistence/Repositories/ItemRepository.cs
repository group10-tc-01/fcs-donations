using Fcg.Donations.Domain.Items;
using MongoDB.Driver;

namespace Fcg.Donations.Infrastructure.MongoDb.Persistence.Repositories;

public sealed class ItemRepository : IItemRepository
{
    private readonly MongoDbContext _context;

    public ItemRepository(MongoDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        return _context.Items.InsertOneAsync(item, cancellationToken: cancellationToken);
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Items.Find(item => item.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Items.Find(item => item.Name == name).AnyAsync(cancellationToken);
    }
}
