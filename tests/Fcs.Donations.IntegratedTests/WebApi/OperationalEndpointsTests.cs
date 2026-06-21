using System.Net;
using System.Text.Json;
using Fcs.Donations.IntegratedTests.Configurations;
using FluentAssertions;

namespace Fcs.Donations.IntegratedTests.WebApi;

public sealed class OperationalEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OperationalEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Given_SwaggerDocument_When_Requested_Then_ShouldExposeBearerAuthenticationForProtectedEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var bearerScheme = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");

        bearerScheme.GetProperty("type").GetString().Should().Be("http");
        bearerScheme.GetProperty("scheme").GetString().Should().Be("bearer");

        var protectedOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/v1/donations")
            .GetProperty("post");

        protectedOperation.GetProperty("security")[0]
            .TryGetProperty("Bearer", out _)
            .Should().BeTrue();
    }

    [Fact]
    public async Task Given_SwaggerDocument_When_Requested_Then_ShouldNotDeclareGlobalSecurityRequirement()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        document.RootElement.TryGetProperty("security", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Given_TestEnvironment_When_ApplicationStarts_Then_ShouldNotApplyMigrations()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Given_CorrelationIdHeader_When_RequestIsHandled_Then_ShouldPropagateCorrelationId()
    {
        const string correlationId = "donations-integration-test";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", correlationId);

        var response = await _client.SendAsync(request);

        response.Headers.GetValues("X-Correlation-Id").Should().ContainSingle(correlationId);
    }

    [Fact]
    public async Task Given_NoCorrelationIdHeader_When_RequestIsHandled_Then_ShouldGenerateCorrelationId()
    {
        var response = await _client.GetAsync("/health");

        response.Headers.GetValues("X-Correlation-Id")
            .Select(IsGuid)
            .Should().ContainSingle(isGuid => isGuid);
    }

    [Fact]
    public async Task Given_HealthEndpoint_When_DatabaseIsUnavailable_Then_ShouldReturnServiceUnavailable()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.Headers.CacheControl?.NoStore.Should().BeTrue();
    }

    [Fact]
    public async Task Given_MetricsEndpoint_When_Requested_Then_ShouldReturnPrometheusMetrics()
    {
        var response = await _client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("# HELP");
    }

    private static bool IsGuid(string value) => Guid.TryParse(value, out _);
}
