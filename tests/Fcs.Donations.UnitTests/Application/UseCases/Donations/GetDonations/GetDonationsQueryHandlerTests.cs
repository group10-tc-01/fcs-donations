using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;
using FluentAssertions;

namespace Fcs.Donations.UnitTests.Application.UseCases.Donations.GetDonations;

public sealed class GetDonationsQueryHandlerTests
{
    [Fact]
    public async Task Given_AuthenticatedDonor_When_Handle_Then_ShouldReturnOnlyOwnDonations()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        var expectedDonation = Donation.Create(Guid.NewGuid(), donorId, 125).Value;
        var otherDonation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 300).Value;
        await repository.AddAsync(expectedDonation);
        await repository.AddAsync(otherDonation);
        var sut = new GetDonationsQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().Id.Should().Be(expectedDonation.Id);
        result.Value.Items.Single().DonorId.Should().Be(donorId);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Given_UnauthenticatedCurrentUser_When_Handle_Then_ShouldReturnFailure()
    {
        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = false,
            KeycloakUserId = null
        };
        var sut = new GetDonationsQueryHandler(new InMemoryDonationRepository(), currentUser);

        var result = await sut.Handle(new GetDonationsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task Given_InvalidCurrentUserId_When_Handle_Then_ShouldReturnFailure()
    {
        var currentUser = new FakeCurrentUser { KeycloakUserId = "invalid-guid" };
        var sut = new GetDonationsQueryHandler(new InMemoryDonationRepository(), currentUser);

        var result = await sut.Handle(new GetDonationsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task Given_MultipleDonations_When_Handle_Then_ShouldReturnPaginatedResults()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        for (var i = 0; i < 5; i++)
        {
            var donation = Donation.Create(Guid.NewGuid(), donorId, 50 * (i + 1)).Value;
            await repository.AddAsync(donation);
        }
        var sut = new GetDonationsQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationsQuery(Page: 1, PageSize: 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Given_StatusFilter_When_Handle_Then_ShouldReturnOnlyMatchingDonations()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        var pending = Donation.Create(Guid.NewGuid(), donorId, 100).Value;
        var processed = Donation.Create(Guid.NewGuid(), donorId, 200).Value;
        processed.MarkProcessed();
        await repository.AddAsync(pending);
        await repository.AddAsync(processed);
        var sut = new GetDonationsQueryHandler(repository, currentUser);

        var result = await sut.Handle(
            new GetDonationsQuery(Status: DonationStatus.Processed),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().Id.Should().Be(processed.Id);
    }

    [Fact]
    public async Task Given_StatusFilterFailed_When_Handle_Then_ShouldReturnOnlyFailedDonations()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        var pending = Donation.Create(Guid.NewGuid(), donorId, 100).Value;
        var failed = Donation.Create(Guid.NewGuid(), donorId, 200).Value;
        failed.MarkFailed("insufficient funds");
        await repository.AddAsync(pending);
        await repository.AddAsync(failed);
        var sut = new GetDonationsQueryHandler(repository, currentUser);

        var result = await sut.Handle(
            new GetDonationsQuery(Status: DonationStatus.Failed),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().Id.Should().Be(failed.Id);
    }

    [Fact]
    public async Task Given_PageZero_When_Handle_Then_ShouldNormalizeToPageOne()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        for (var i = 0; i < 3; i++)
        {
            var donation = Donation.Create(Guid.NewGuid(), donorId, 50).Value;
            await repository.AddAsync(donation);
        }
        var sut = new GetDonationsQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationsQuery(Page: 0), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(1);
        result.Value.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Given_PageSizeZero_When_Handle_Then_ShouldNormalizeToDefault()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        for (var i = 0; i < 3; i++)
        {
            var donation = Donation.Create(Guid.NewGuid(), donorId, 50).Value;
            await repository.AddAsync(donation);
        }
        var sut = new GetDonationsQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationsQuery(PageSize: 0), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Given_PageSizeAboveMax_When_Handle_Then_ShouldNormalizeToDefault()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        for (var i = 0; i < 3; i++)
        {
            var donation = Donation.Create(Guid.NewGuid(), donorId, 50).Value;
            await repository.AddAsync(donation);
        }
        var sut = new GetDonationsQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationsQuery(PageSize: 101), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(10);
    }
}
