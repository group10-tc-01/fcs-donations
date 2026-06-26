using Fcs.Donations.Application.UseCases.Donations.GetAdminDonations;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.Donations;
using FluentAssertions;

namespace Fcs.Donations.UnitTests.Application.UseCases.Donations.GetAdminDonations;

public sealed class GetAdminDonationsQueryHandlerTests
{
    [Fact]
    public async Task Given_ExistingDonations_When_Handle_Then_ShouldReturnAllDonations()
    {
        var repository = new InMemoryDonationRepository();
        var donationA = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 100).Value;
        var donationB = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 200).Value;
        await repository.AddAsync(donationA);
        await repository.AddAsync(donationB);
        var sut = new GetAdminDonationsQueryHandler(repository);

        var result = await sut.Handle(new GetAdminDonationsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_StatusFilter_When_Handle_Then_ShouldReturnOnlyMatchingDonations()
    {
        var repository = new InMemoryDonationRepository();
        var pending = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 100).Value;
        var processed = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 200).Value;
        processed.MarkProcessed();
        await repository.AddAsync(pending);
        await repository.AddAsync(processed);
        var sut = new GetAdminDonationsQueryHandler(repository);

        var result = await sut.Handle(
            new GetAdminDonationsQuery(Status: DonationStatus.Processed),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().Id.Should().Be(processed.Id);
    }

    [Fact]
    public async Task Given_MultipleDonations_When_Handle_Then_ShouldReturnPaginatedResults()
    {
        var repository = new InMemoryDonationRepository();
        for (var i = 0; i < 5; i++)
        {
            var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 50 * (i + 1)).Value;
            await repository.AddAsync(donation);
        }
        var sut = new GetAdminDonationsQueryHandler(repository);

        var result = await sut.Handle(
            new GetAdminDonationsQuery(Page: 1, PageSize: 2),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalCount.Should().Be(5);
    }
}
