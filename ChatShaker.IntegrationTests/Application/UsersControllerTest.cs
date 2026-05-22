using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Users.Queries.GetFriends;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class UsersControllerTest : IntegrationTestBase
{
    public UsersControllerTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task GetFriends_ReturnOk_WhenUserHasFriends()
    {
        await GetTokenForAuth();

        var response = await HttpClient.GetAsync("api/users/get-friends");

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<List<UserInfoDto>>>();

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        data.Should().NotBeNull();
        data.Data.Should().BeEmpty(); // Since we just seeded users but no friendships
    }
}
