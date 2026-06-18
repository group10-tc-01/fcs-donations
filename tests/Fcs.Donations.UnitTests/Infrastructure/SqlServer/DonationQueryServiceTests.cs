using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Infrastructure.SqlServer.Persistence;
using Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.UnitTests.Infrastructure.SqlServer;

public sealed class DonationQueryServiceTests
{
    [Fact]
    public async Task Given_DonationsFromMultipleDonors_When_QueryByDonor_Then_ShouldReturnProjectedDonationsFromRequestedDonor()
    {
        var options = new DbContextOptionsBuilder<FcsDonationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var donorId = Guid.NewGuid();
        var expectedDonation = Donation.Create(Guid.NewGuid(), donorId, 125).Value;
        var otherDonation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 300).Value;

        await using var dbContext = new FcsDonationsDbContext(options);
        await dbContext.Donations.AddRangeAsync(expectedDonation, otherDonation);
        await dbContext.SaveChangesAsync();

        var sut = new DonationQueryService(dbContext);

        var result = sut.QueryByDonor(donorId).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(expectedDonation.Id);
        result[0].CampaignId.Should().Be(expectedDonation.CampaignId);
        result[0].DonorId.Should().Be(donorId);
        result[0].Amount.Should().Be(expectedDonation.Amount);
        result[0].Status.Should().Be(expectedDonation.Status);
        result[0].CreatedAt.Should().Be(expectedDonation.CreatedAt);
    }
}
