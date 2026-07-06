using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;
using FluentAssertions;

namespace Fcs.Donations.UnitTests.Domain.Donations;

public sealed class DonationTests
{
    [Fact]
    public void Given_InvalidAmount_When_Create_Then_ShouldReturnValidationError()
    {
        var result = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Given_InvalidCampaignId_When_Create_Then_ShouldReturnValidationError()
    {
        var result = Donation.Create(Guid.Empty, Guid.NewGuid(), 10);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Given_ValidData_When_Create_Then_ShouldReturnSuccess()
    {
        var campaignId = Guid.NewGuid();
        var donorId = Guid.NewGuid();

        var result = Donation.Create(campaignId, donorId, 100.50m);

        result.IsSuccess.Should().BeTrue();
        result.Value.CampaignId.Should().Be(campaignId);
        result.Value.DonorId.Should().Be(donorId);
        result.Value.Amount.Should().Be(100.50m);
        result.Value.Status.Should().Be(DonationStatus.Pending);
    }

    [Fact]
    public void Given_PendingDonation_When_MarkProcessed_Then_ShouldUpdateStatus()
    {
        var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 50).Value;

        donation.MarkProcessed();

        donation.Status.Should().Be(DonationStatus.Processed);
        donation.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Given_PendingDonation_When_MarkFailed_Then_ShouldUpdateStatus()
    {
        var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 50).Value;

        donation.MarkFailed("Insufficient funds");

        donation.Status.Should().Be(DonationStatus.Failed);
        donation.FailureReason.Should().Be("Insufficient funds");
    }

    [Fact]
    public void Given_LongFailureReason_When_MarkFailed_Then_ShouldTruncateTo1000()
    {
        var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 50).Value;
        var longReason = new string('x', 1500);

        donation.MarkFailed(longReason);

        donation.FailureReason.Should().HaveLength(1000);
        donation.FailureReason.Should().Be(longReason[..1000]);
    }
}
