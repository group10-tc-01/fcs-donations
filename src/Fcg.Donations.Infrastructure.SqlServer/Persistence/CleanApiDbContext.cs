using Fcg.Donations.Domain.Abstractions;
using Fcg.Donations.Domain.Donations;
using Fcg.Donations.Domain.Items;
using Fcg.Donations.Domain.OutboxMessages;
using Fcg.Donations.Domain.ProcessedMessages;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Donations.Infrastructure.SqlServer.Persistence;

public sealed class CleanApiDbContext : DbContext, IUnitOfWork
{
    public CleanApiDbContext(DbContextOptions<CleanApiDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CleanApiDbContext).Assembly);
    }
}
