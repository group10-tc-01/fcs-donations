using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.DependencyInjection;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Domain.Donations;

using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Fcs.Donations.UnitTests.Application;

public sealed class ApplicationSupportTests
{
    [Fact]
    public void Given_InvalidCreateDonationRequest_When_Validate_Then_ShouldReturnErrors()
    {
        var validator = new CreateDonationRequestValidator();
        var request = new CreateDonationRequest(Guid.Empty, 0);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Given_DonationQueryResponse_When_Initialized_Then_ShouldKeepValues()
    {
        var id = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var donorId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var processedAt = createdAt.AddMinutes(1);

        var response = new DonationQueryResponse
        {
            Id = id,
            CampaignId = campaignId,
            DonorId = donorId,
            Amount = 100,
            Status = DonationStatus.Processed,
            CreatedAt = createdAt,
            ProcessedAt = processedAt,
            FailureReason = "none"
        };

        response.Id.Should().Be(id);
        response.CampaignId.Should().Be(campaignId);
        response.DonorId.Should().Be(donorId);
        response.Amount.Should().Be(100);
        response.Status.Should().Be(DonationStatus.Processed);
        response.CreatedAt.Should().Be(createdAt);
        response.ProcessedAt.Should().Be(processedAt);
        response.FailureReason.Should().Be("none");
    }

    [Fact]

    public void Given_ServiceCollection_When_AddApplication_Then_ShouldRegisterApplicationServices()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        using var provider = services.BuildServiceProvider();

        provider.GetServices<IValidator<CreateDonationRequest>>().Should().NotBeEmpty();

        services.Should().NotContain(descriptor => descriptor.ServiceType == typeof(IMessagePublisher));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IValidator<CreateDonationRequest>));
    }
}
