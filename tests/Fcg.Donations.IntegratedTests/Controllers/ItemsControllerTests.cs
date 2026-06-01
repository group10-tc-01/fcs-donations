using Fcg.Donations.Application.UseCases.Items.CreateItem;
using Fcg.Donations.CommomTestsUtilities.Builders.Items;
using Fcg.Donations.IntegratedTests.Configurations;
using Fcg.Donations.WebApi.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Fcg.Donations.IntegratedTests.Controllers;

public sealed class ItemsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Given_ValidRequest_When_PostIsCalled_Then_ShouldReturnCreated()
    {
        var request = new CreateItemRequestBuilder().Build();

        var response = await _client.PostAsJsonAsync("/api/v1/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CreateItemResponse>>();
        payload.Should().NotBeNull();
        payload!.Data!.Id.Should().NotBeEmpty();
    }
}
