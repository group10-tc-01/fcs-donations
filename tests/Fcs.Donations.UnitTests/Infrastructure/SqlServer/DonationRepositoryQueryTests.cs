using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Infrastructure.SqlServer.Persistence;
using Fcs.Donations.Infrastructure.SqlServer.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.UnitTests.Infrastructure.SqlServer;

public sealed class DonationRepositoryQueryTests
{
    [Fact]
    public async Task Given_PersistedDonations_When_Query_Then_ShouldExposeQueryableDonations()
    {
        var options = new DbContextOptionsBuilder<FcsDonationsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 125).Value;

        await using var dbContext = new FcsDonationsDbContext(options);
        await dbContext.Donations.AddAsync(donation);
        await dbContext.SaveChangesAsync();
        var sut = new DonationRepository(dbContext);

        var result = sut.Query().ToList();

        result.Should().ContainSingle(item => item.Id == donation.Id);
    }
}
