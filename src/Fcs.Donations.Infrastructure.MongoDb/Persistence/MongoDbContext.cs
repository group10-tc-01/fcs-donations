using Fcs.Donations.Domain.Donations;
using MongoDB.Driver;

namespace Fcs.Donations.Infrastructure.MongoDb.Persistence;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<Donation> Donations => _database.GetCollection<Donation>("donations");
}
