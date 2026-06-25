using Fcs.Donations.Domain.Abstractions;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.Infrastructure.SqlServer.Persistence;

public sealed class FcsDonationsDbContext : DbContext, IUnitOfWork
{
    public FcsDonationsDbContext(DbContextOptions<FcsDonationsDbContext> options) : base(options)
    {
    }

    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FcsDonationsDbContext).Assembly);
    }
}
