using Fcs.Donations.Application.UseCases.Donations.GetDonationById;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;
using FluentAssertions;

namespace Fcs.Donations.UnitTests.Application.UseCases.Donations.GetDonationById;

public sealed class GetDonationByIdQueryHandlerTests
{
    [Fact]
    public async Task Given_AuthenticatedDonorOwnDonation_When_Handle_Then_ShouldReturnDonation()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donorId = Guid.Parse(currentUser.KeycloakUserId!);
        var donation = Donation.Create(Guid.NewGuid(), donorId, 125).Value;
        await repository.AddAsync(donation);
        var sut = new GetDonationByIdQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationByIdQuery(donation.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(donation.Id);
        result.Value.DonorId.Should().Be(donorId);
    }

    [Fact]
    public async Task Given_DonationFromAnotherDonor_When_Handle_Then_ShouldReturnNotFound()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser();
        var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 125).Value;
        await repository.AddAsync(donation);
        var sut = new GetDonationByIdQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationByIdQuery(donation.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Given_GestorONG_When_Handle_Then_ShouldReturnAnyDonation()
    {
        var repository = new InMemoryDonationRepository();
        var currentUser = new FakeCurrentUser { Roles = ["GestorONG"] };
        var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 125).Value;
        await repository.AddAsync(donation);
        var sut = new GetDonationByIdQueryHandler(repository, currentUser);

        var result = await sut.Handle(new GetDonationByIdQuery(donation.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(donation.Id);
    }

    [Fact]
    public async Task Given_MissingDonation_When_Handle_Then_ShouldReturnNotFound()
    {
        var sut = new GetDonationByIdQueryHandler(new InMemoryDonationRepository(), new FakeCurrentUser());

        var result = await sut.Handle(new GetDonationByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
