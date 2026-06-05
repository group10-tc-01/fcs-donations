using fcs.Donations.Domain.Abstractions;
using fcs.Donations.Domain.Donations;
using fcs.Donations.Domain.Items;
using fcs.Donations.Domain.OutboxMessages;
using fcs.Donations.Domain.ProcessedMessages;
using Microsoft.EntityFrameworkCore;

namespace fcs.Donations.Infrastructure.SqlServer.Persistence;

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
