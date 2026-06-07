using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.DependencyInjection;
using Fcs.Donations.Application.Messaging;
using Fcs.Donations.Application.Settings;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
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
    public void Given_ServiceCollection_When_AddApplication_Then_ShouldRegisterApplicationServices()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IMessagePublisher) &&
            descriptor.ImplementationType == typeof(NullMessagePublisher));
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IValidator<CreateDonationRequest>));
    }
}
