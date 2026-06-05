using fcs.Donations.Application.UseCases.Donations.CreateDonation;
using fcs.Donations.CommomTestsUtilities.Builders.Donations;
using fcs.Donations.CommomTestsUtilities.TestDoubles;
using fcs.Donations.Domain;
using FluentAssertions;

namespace fcs.Donations.UnitTests.Application.UseCases.Donations.CreateDonation;

public sealed class CreateDonationUseCaseTests
{
    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ShouldCreateDonation()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var loggedUser = new FakeLoggedUserService();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, loggedUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Amount.Should().Be(request.Amount);
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_CampaignNotEligible_When_Handle_Then_ShouldReturnConflictError()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient { IsEligible = false };
        var loggedUser = new FakeLoggedUserService();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, loggedUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
