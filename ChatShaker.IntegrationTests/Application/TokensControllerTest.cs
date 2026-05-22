using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Tokens.Commands.CreateConfirmationToken;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;
using System.Net;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class TokensControllerTest : IntegrationTestBase
{
    public TokensControllerTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateNewActivationToken_ReturnOk_WhenEmailExists()
    {
        var dto = new CreateConfirmationTokenDto { Email = "test@test.com" };

        var response = await HttpClient.PostAsJsonAsync("api/tokens/create-new-activation-token", dto);

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<object>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateNewActivationToken_ReturnBadRequest_WhenEmailNotExists()
    {
        var dto = new CreateConfirmationTokenDto { Email = "nonexistent@test.com" };

        var response = await HttpClient.PostAsJsonAsync("api/tokens/create-new-activation-token", dto);

        var data = await response.Content.ReadFromJsonAsync<Response<object>>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        data.Errors.Should().NotBeNullOrEmpty();
    }
}
