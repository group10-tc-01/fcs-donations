using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.Domain.ProcessedMessages;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.Infrastructure.SqlServer.Persistence;

public sealed class CleanApiDbContext : DbContext, IUnitOfWork
{
    public CleanApiDbContext(DbContextOptions<CleanApiDbContext> options) : base(options)
    {
    }

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
