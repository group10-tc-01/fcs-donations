using Fcs.Donations.Domain.Results;
using FluentAssertions;

namespace Fcs.Donations.UnitTests.Domain.Results;

public sealed class ResultTests
{
    [Fact]
    public void Given_SuccessResult_When_AccessingValue_Then_ShouldReturnValue()
    {
        Result<string> result = "created";

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("created");
    }

    [Fact]
    public void Given_FailureResult_When_AccessingError_Then_ShouldReturnError()
    {
        var error = Error.NotFound("Donation.NotFound", "Donation not found.");
        Result<string> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Given_FailureResult_When_AccessingValue_Then_ShouldThrow()
    {
        Result<string> result = Error.Failure("Failure", "Failure message.");

        var action = () => result.Value;

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Given_SuccessResult_When_AccessingError_Then_ShouldThrow()
    {
        var result = Result<string>.Success("created");

        var action = () => result.Error;

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Given_ErrorFactoryMethods_When_Called_Then_ShouldSetExpectedTypes()
    {
        Error.None.Type.Should().Be(ErrorType.Failure);
        Error.Failure("Failure", "Failure message.").Type.Should().Be(ErrorType.Failure);
        Error.Validation("Validation", "Validation message.").Type.Should().Be(ErrorType.Validation);
        Error.NotFound("NotFound", "Not found message.").Type.Should().Be(ErrorType.NotFound);
        Error.Conflict("Conflict", "Conflict message.").Type.Should().Be(ErrorType.Conflict);
    }
}
