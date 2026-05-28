using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Friendships.Query;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;
using System.Net;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class FriendshipControllerTest : IntegrationTestBase
{
    public FriendshipControllerTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task GetRecieved_ReturnOk()
    {
        await GetTokenForAuth();

        var response = await HttpClient.GetAsync("api/friendship/get-recieved");

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<List<UserRequestsDto>>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSent_ReturnOk()
    {
        await GetTokenForAuth();

        var response = await HttpClient.GetAsync("api/friendship/get-sent");

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<List<UserRequestsDto>>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Data.Should().NotBeNull();
    }
}
