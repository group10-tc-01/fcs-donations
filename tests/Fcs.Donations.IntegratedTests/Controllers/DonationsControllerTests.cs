using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.CommomTestsUtilities.Builders.Donations;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.IntegratedTests.Configurations;
using Fcs.Donations.WebApi.Models;
using FluentAssertions;

namespace Fcs.Donations.IntegratedTests.Controllers;

public sealed class DonationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public DonationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.DonationRepository.Clear();
        _factory.LoggedUser.UserId = Guid.NewGuid();

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

    [Fact]
    public async Task Given_ExistingDonations_When_GetIsCalled_Then_ShouldReturnOnlyLoggedDonorDonations()
    {
        var loggedDonorId = _factory.LoggedUser.UserId!.Value;
        var otherDonorId = Guid.NewGuid();
        var expectedDonation = Donation.Create(Guid.NewGuid(), loggedDonorId, 120).Value;
        var otherDonation = Donation.Create(Guid.NewGuid(), otherDonorId, 200).Value;

        await _factory.DonationRepository.AddAsync(expectedDonation);
        await _factory.DonationRepository.AddAsync(otherDonation);

        var response = await _client.GetAsync("/api/v1/donations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var donations = GetDonationArray(document);

        donations.GetArrayLength().Should().Be(1);
        donations[0].GetProperty("id").GetGuid().Should().Be(expectedDonation.Id);
        donations[0].GetProperty("donorId").GetGuid().Should().Be(loggedDonorId);
    }

    [Fact]
    public async Task Given_ODataQuery_When_GetIsCalled_Then_ShouldApplyFilterOrderAndTop()
    {
        var loggedDonorId = _factory.LoggedUser.UserId!.Value;
        var lowerDonation = Donation.Create(Guid.NewGuid(), loggedDonorId, 50).Value;
        var middleDonation = Donation.Create(Guid.NewGuid(), loggedDonorId, 150).Value;
        var higherDonation = Donation.Create(Guid.NewGuid(), loggedDonorId, 250).Value;

        await _factory.DonationRepository.AddAsync(lowerDonation);
        await _factory.DonationRepository.AddAsync(middleDonation);
        await _factory.DonationRepository.AddAsync(higherDonation);

        var response = await _client.GetAsync("/api/v1/donations?$filter=Amount gt 100&$orderby=Amount desc&$top=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var donations = GetDonationArray(document);

        donations.GetArrayLength().Should().Be(1);
        donations[0].GetProperty("id").GetGuid().Should().Be(higherDonation.Id);
        donations[0].GetProperty("amount").GetDecimal().Should().Be(250);
    }

    [Fact]
    public async Task Given_MissingLoggedUser_When_GetIsCalled_Then_ShouldReturnUnauthorized()
    {
        _factory.LoggedUser.UserId = null;

        var response = await _client.GetAsync("/api/v1/donations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        payload.Should().NotBeNull();
        payload!.Success.Should().BeFalse();
    }

    private static JsonElement GetDonationArray(JsonDocument document)
    {
        var root = document.RootElement;

        return root.ValueKind == JsonValueKind.Array
            ? root
            : root.GetProperty("value");
    }
}
