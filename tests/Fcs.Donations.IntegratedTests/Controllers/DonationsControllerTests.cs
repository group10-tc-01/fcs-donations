using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.CommomTestsUtilities.Builders.Donations;
using Fcs.Donations.Domain.Donations;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.IntegratedTests.Configurations;
using FluentAssertions;

namespace Fcs.Donations.IntegratedTests.Controllers;

public sealed class DonationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DonationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.DonationRepository.Clear();
        _factory.CampaignClient.Error = null;
        _factory.CampaignClient.IsEligible = true;
        _factory.CurrentUser.IsAuthenticated = true;
        _factory.CurrentUser.KeycloakUserId = Guid.NewGuid().ToString();

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
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<CreateDonationResponse>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Given_CampaignNotFound_When_PostIsCalled_Then_ShouldReturnNotFound()
    {
        _factory.CampaignClient.Error = Error.NotFound(
            "Campaign.NotFound",
            "Campaign was not found.");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/donations",
            new CreateDonationRequestBuilder().Build());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<string>>(JsonOptions);
        payload!.Message.Should().Be("Campaign was not found.");
    }

    [Fact]
    public async Task Given_ClosedCampaign_When_PostIsCalled_Then_ShouldReturnBadRequest()
    {
        _factory.CampaignClient.IsEligible = false;

        var response = await _client.PostAsJsonAsync(
            "/api/v1/donations",
            new CreateDonationRequestBuilder().Build());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Given_CampaignServiceUnavailable_When_PostIsCalled_Then_ShouldReturnServiceUnavailable()
    {
        _factory.CampaignClient.Error = Error.ServiceUnavailable(
            "Campaign.ServiceUnavailable",
            "Campaign service is temporarily unavailable.");

        var response = await _client.PostAsJsonAsync(
            "/api/v1/donations",
            new CreateDonationRequestBuilder().Build());

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<string>>(JsonOptions);
        payload!.Message.Should().Be("Campaign service is temporarily unavailable.");
    }

    [Fact]
    public async Task Given_ExistingDonations_When_GetIsCalled_Then_ShouldReturnOnlyLoggedDonorDonations()
    {
        var loggedDonorId = Guid.Parse(_factory.CurrentUser.KeycloakUserId!);
        var otherDonorId = Guid.NewGuid();
        var expectedDonation = Donation.Create(Guid.NewGuid(), loggedDonorId, 120).Value;
        var otherDonation = Donation.Create(Guid.NewGuid(), otherDonorId, 200).Value;

        await _factory.DonationRepository.AddAsync(expectedDonation);
        await _factory.DonationRepository.AddAsync(otherDonation);

        var response = await _client.GetAsync("/api/v1/donations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<PagedData<DonationItem>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Success.Should().BeTrue();
        payload.Data.Should().NotBeNull();
        payload.Data!.Items.Should().ContainSingle();
        payload.Data.Items.Single().Id.Should().Be(expectedDonation.Id);
        payload.Data.Items.Single().DonorId.Should().Be(loggedDonorId);
        payload.Data.Page.Should().Be(1);
        payload.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Given_MultipleDonations_When_GetIsCalledWithPagination_Then_ShouldReturnCorrectPage()
    {
        var loggedDonorId = Guid.Parse(_factory.CurrentUser.KeycloakUserId!);
        for (var i = 0; i < 5; i++)
        {
            var donation = Donation.Create(Guid.NewGuid(), loggedDonorId, 50 * (i + 1)).Value;
            await _factory.DonationRepository.AddAsync(donation);
        }

        var response = await _client.GetAsync("/api/v1/donations?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<PagedData<DonationItem>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Items.Count.Should().Be(2);
        payload.Data.Page.Should().Be(1);
        payload.Data.PageSize.Should().Be(2);
        payload.Data.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Given_StatusFilter_When_GetIsCalled_Then_ShouldReturnFilteredDonations()
    {
        var loggedDonorId = Guid.Parse(_factory.CurrentUser.KeycloakUserId!);
        var pending = Donation.Create(Guid.NewGuid(), loggedDonorId, 100).Value;
        var processed = Donation.Create(Guid.NewGuid(), loggedDonorId, 200).Value;
        processed.MarkProcessed();
        var failed = Donation.Create(Guid.NewGuid(), loggedDonorId, 300).Value;
        failed.MarkFailed("error");

        await _factory.DonationRepository.AddAsync(pending);
        await _factory.DonationRepository.AddAsync(processed);
        await _factory.DonationRepository.AddAsync(failed);

        var response = await _client.GetAsync("/api/v1/donations?status=Processed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<PagedData<DonationItem>>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Items.Should().ContainSingle();
        payload.Data.Items.Single().Id.Should().Be(processed.Id);
    }

    [Fact]
    public async Task Given_OwnDonation_When_GetByIdIsCalled_Then_ShouldReturnDonation()
    {
        var loggedDonorId = Guid.Parse(_factory.CurrentUser.KeycloakUserId!);
        var donation = Donation.Create(Guid.NewGuid(), loggedDonorId, 120).Value;
        await _factory.DonationRepository.AddAsync(donation);

        var response = await _client.GetAsync($"/api/v1/donations/{donation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<DonationItem>>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.Data!.Id.Should().Be(donation.Id);
        payload.Data.DonorId.Should().Be(loggedDonorId);
    }

    [Fact]
    public async Task Given_DonationFromAnotherDonor_When_GetByIdIsCalled_Then_ShouldReturnNotFound()
    {
        var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 120).Value;
        await _factory.DonationRepository.AddAsync(donation);

        var response = await _client.GetAsync($"/api/v1/donations/{donation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_MissingDonation_When_GetByIdIsCalled_Then_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/donations/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Given_UnauthenticatedCurrentUser_When_GetIsCalled_Then_ShouldReturnUnauthorized()
    {
        _factory.CurrentUser.IsAuthenticated = false;
        _factory.CurrentUser.KeycloakUserId = null;

        var response = await _client.GetAsync("/api/v1/donations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public sealed class GestorONGListTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GestorONGListTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.DonationRepository.Clear();
            _factory.CurrentUser.IsAuthenticated = true;
            _factory.CurrentUser.KeycloakUserId = Guid.NewGuid().ToString();
            _factory.CurrentUser.Roles = ["GestorONG"];

            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthTestHelper.GenerateToken("GestorONG"));
        }

        [Fact]
        public async Task Given_GestorONGRole_When_GetIsCalled_Then_ShouldReturnAllDonations()
        {
            var donorA = Guid.NewGuid();
            var donorB = Guid.NewGuid();
            var donationA = Donation.Create(Guid.NewGuid(), donorA, 100).Value;
            var donationB = Donation.Create(Guid.NewGuid(), donorB, 200).Value;

            await _factory.DonationRepository.AddAsync(donationA);
            await _factory.DonationRepository.AddAsync(donationB);

            var response = await _client.GetAsync("/api/v1/donations");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<PagedData<DonationItem>>>(JsonOptions);
            payload.Should().NotBeNull();
            payload!.Data!.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task Given_StatusFilter_When_GetIsCalledByGestorONG_Then_ShouldReturnFilteredDonations()
        {
            var donorId = Guid.NewGuid();
            var pending = Donation.Create(Guid.NewGuid(), donorId, 100).Value;
            var processed = Donation.Create(Guid.NewGuid(), donorId, 200).Value;
            processed.MarkProcessed();

            await _factory.DonationRepository.AddAsync(pending);
            await _factory.DonationRepository.AddAsync(processed);

            var response = await _client.GetAsync("/api/v1/donations?status=Pending");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<PagedData<DonationItem>>>(JsonOptions);
            payload.Should().NotBeNull();
            payload!.Data!.Items.Should().ContainSingle();
            payload.Data.Items.Single().Id.Should().Be(pending.Id);
        }

        [Fact]
        public async Task Given_GestorONGRole_When_GetByIdIsCalled_Then_ShouldReturnAnyDonation()
        {
            var donation = Donation.Create(Guid.NewGuid(), Guid.NewGuid(), 100).Value;
            await _factory.DonationRepository.AddAsync(donation);

            var response = await _client.GetAsync($"/api/v1/donations/{donation.Id}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<ApiEnvelope<DonationItem>>(JsonOptions);
            payload.Should().NotBeNull();
            payload!.Data!.Id.Should().Be(donation.Id);
        }
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    private sealed class PagedData<T>
    {
        public List<T> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    private sealed class DonationItem
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public Guid DonorId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
