using System.Net;
using System.Text;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Infrastructure.Http.CampaignEligibility;
using Fcs.Donations.Messages;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Refit;

namespace Fcs.Donations.UnitTests.Infrastructure.Http;

public sealed class CampaignEligibilityClientTests
{
    [Fact]
    public async Task Given_SuccessEnvelope_When_CheckEligibility_Then_ShouldMapNestedData()
    {
        const string json = """
            {
              "success": true,
              "data": {
                "campaignId": "a61ed90a-f06f-42a7-812c-58f5d398547b",
                "eligible": false,
                "reason": "Campaign status is Completed."
              },
              "message": null
            }
            """;
        var sut = CreateClient(HttpStatusCode.OK, json);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEligible.Should().BeFalse();
        result.Value.Reason.Should().Be("Campaign status is Completed.");
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, ErrorType.Validation)]
    [InlineData(HttpStatusCode.NotFound, ErrorType.NotFound)]
    [InlineData(HttpStatusCode.Conflict, ErrorType.Validation)]
    public async Task Given_BusinessErrorEnvelope_When_CheckEligibility_Then_ShouldPreserveMessage(
        HttpStatusCode statusCode,
        ErrorType expectedErrorType)
    {
        const string json = """
            {
              "success": false,
              "data": null,
              "message": "Campaign was not found."
            }
            """;
        var sut = CreateClient(statusCode, json);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(expectedErrorType);
        result.Error.Message.Should().Be("Campaign was not found.");
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Given_TransientHttpFailure_When_CheckEligibility_Then_ShouldReturnServiceUnavailable(
        HttpStatusCode statusCode)
    {
        var sut = CreateClient(statusCode, string.Empty);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
    }

    [Fact]
    public async Task Given_HttpTimeout_When_CheckEligibility_Then_ShouldReturnServiceUnavailable()
    {
        var api = new ThrowingCampaignEligibilityApi(new TaskCanceledException("timeout"));
        var sut = new CampaignEligibilityClient(api, NullLogger<CampaignEligibilityClient>.Instance);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
    }

    [Fact]
    public async Task Given_CallerCancellation_When_CheckEligibility_Then_ShouldPropagateCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var api = new ThrowingCampaignEligibilityApi(new OperationCanceledException(cancellationTokenSource.Token));
        var sut = new CampaignEligibilityClient(api, NullLogger<CampaignEligibilityClient>.Instance);

        var action = () => sut.CheckEligibilityAsync(Guid.NewGuid(), cancellationTokenSource.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Given_SuccessEnvelopeWithoutData_When_CheckEligibility_Then_ShouldReturnServiceUnavailable()
    {
        const string json = """
            {
              "success": true,
              "data": null,
              "message": null
            }
            """;
        var sut = CreateClient(HttpStatusCode.OK, json);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
    }

    [Fact]
    public async Task Given_SuccessEnvelopeWithSuccessFalse_When_CheckEligibility_Then_ShouldReturnValidationError()
    {
        const string json = """
            {
              "success": false,
              "data": null,
              "message": "Campaign eligibility request was rejected."
            }
            """;
        var sut = CreateClient(HttpStatusCode.OK, json);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Be("Campaign eligibility request was rejected.");
    }

    [Fact]
    public async Task Given_HttpRequestException_When_CheckEligibility_Then_ShouldReturnServiceUnavailable()
    {
        var api = new ThrowingCampaignEligibilityApi(new HttpRequestException("Connection refused."));
        var sut = new CampaignEligibilityClient(api, NullLogger<CampaignEligibilityClient>.Instance);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
    }

    [Fact]
    public async Task Given_InvalidJsonResponse_When_CheckEligibility_Then_ShouldReturnServiceUnavailable()
    {
        var api = new ThrowingCampaignEligibilityApi(new System.Text.Json.JsonException("Unexpected token."));
        var sut = new CampaignEligibilityClient(api, NullLogger<CampaignEligibilityClient>.Instance);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
    }

    [Fact]
    public async Task Given_ApiExceptionWithNonJsonBody_When_CheckEligibility_Then_ShouldUseDefaultMessage()
    {
        const string nonJsonBody = "Bad Request";
        var sut = CreateClient(HttpStatusCode.BadRequest, nonJsonBody);

        var result = await sut.CheckEligibilityAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Message.Should().Be(Fcs.Donations.Messages.ResourceMessages.CampaignRequestRejected);
    }

    private static CampaignEligibilityClient CreateClient(HttpStatusCode statusCode, string content)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(statusCode, content))
        {
            BaseAddress = new Uri("http://campaigns-api")
        };
        var api = RestService.For<ICampaignEligibilityApi>(httpClient);

        return new CampaignEligibilityClient(api, NullLogger<CampaignEligibilityClient>.Instance);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public StubHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                RequestMessage = request,
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ThrowingCampaignEligibilityApi : ICampaignEligibilityApi
    {
        private readonly Exception _exception;

        public ThrowingCampaignEligibilityApi(Exception exception)
        {
            _exception = exception;
        }

        public Task<CampaignEligibilityApiResponse> CheckEligibilityAsync(
            Guid campaignId,
            CancellationToken cancellationToken)
        {
            return Task.FromException<CampaignEligibilityApiResponse>(_exception);
        }
    }
}
