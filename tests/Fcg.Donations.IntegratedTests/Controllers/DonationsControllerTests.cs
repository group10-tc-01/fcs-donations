using Fcg.Donations.Application.UseCases.Donations.CreateDonation;
using Fcg.Donations.CommomTestsUtilities.Builders.Donations;
using Fcg.Donations.IntegratedTests.Configurations;
using Fcg.Donations.WebApi.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Fcg.Donations.IntegratedTests.Controllers;

public sealed class DonationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DonationsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AuthTestHelper.GenerateToken());
    }

    [Fact]
    public async Task Given_ValidRequest_When_PostIsCalled_Then_ShouldReturnAccepted()
    {
        var request = new CreateDonationRequestBuilder().Build();

        var response = await _client.PostAsJsonAsync("/api/v1/donations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CreateDonationResponse>>();
        payload.Should().NotBeNull();
        payload!.Data!.Id.Should().NotBeEmpty();
    }
}
