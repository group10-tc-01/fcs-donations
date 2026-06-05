using fcs.Donations.Domain.Donations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace fcs.Donations.Infrastructure.SqlServer.Persistence.Configurations.Donations;

public sealed class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.ToTable("Donations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CampaignId).IsRequired();
        builder.Property(x => x.DonorId).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ProcessedAt);
        builder.Property(x => x.FailureReason).HasMaxLength(500);
        builder.HasIndex(x => x.CampaignId);
        builder.HasIndex(x => x.DonorId);
    }
}
