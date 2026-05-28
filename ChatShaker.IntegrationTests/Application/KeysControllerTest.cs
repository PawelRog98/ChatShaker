using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Users.Commands.SaveIdentity;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;
using System.Net;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class KeysControllerTest : IntegrationTestBase
{
    public KeysControllerTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task GetPublicIdentities_ReturnOk()
    {
        await GetTokenForAuth();
        var users = await GetSeededUsersByNumber(1);
        var userIds = users.Select(u => u.PublicId).ToList();

        var response = await HttpClient.GetAsync($"api/keys/get-public-identities?userIds={userIds[0]}");

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<List<UserKeyDataDto>>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Data.Should().NotBeNull();
    }
}
