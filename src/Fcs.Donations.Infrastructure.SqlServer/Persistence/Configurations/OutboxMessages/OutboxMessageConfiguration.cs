using fcs.Donations.Domain.OutboxMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace fcs.Donations.Infrastructure.SqlServer.Persistence.Configurations.OutboxMessages;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AggregateId).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.PublishedAt);
        builder.Property(x => x.RetryCount).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(1000);
        builder.HasIndex(x => x.Status);
    }
}
