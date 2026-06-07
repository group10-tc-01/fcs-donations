using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.DependencyInjection;
using Fcs.Donations.Application.Messaging;
using Fcs.Donations.Application.Settings;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Domain.Donations;


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
    public async Task Given_NullMessagePublisher_When_PublishAsync_Then_ShouldComplete()
    {
        var publisher = new NullMessagePublisher();

        var action = async () => await publisher.PublishAsync(new { EventId = Guid.NewGuid() });

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public void Given_JwtSettings_When_PropertiesAreSet_Then_ShouldKeepValues()
    {
        var settings = new JwtSettings
        {
            SecretKey = "secret",
            Issuer = "issuer",
            Audience = "audience",
            AccessTokenExpirationMinutes = 10
        };

        settings.SecretKey.Should().Be("secret");
        settings.Issuer.Should().Be("issuer");
        settings.Audience.Should().Be("audience");
        settings.AccessTokenExpirationMinutes.Should().Be(10);
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

        provider.GetRequiredService<IMessagePublisher>().Should().BeOfType<NullMessagePublisher>();
        provider.GetServices<IValidator<CreateDonationRequest>>().Should().NotBeEmpty()
          
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IMessagePublisher) &&
            descriptor.ImplementationType == typeof(NullMessagePublisher));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IValidator<CreateDonationRequest>));
    }
}
