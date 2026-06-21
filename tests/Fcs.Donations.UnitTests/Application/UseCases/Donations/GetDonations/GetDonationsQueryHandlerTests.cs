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
        result.Value.Should().ContainSingle();
        result.Value.Single().Id.Should().Be(expectedDonation.Id);
        result.Value.Single().DonorId.Should().Be(donorId);
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
}
