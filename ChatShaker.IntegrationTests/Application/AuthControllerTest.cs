using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Register;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class AuthControllerTest : IntegrationTestBase
{
    public AuthControllerTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_GetToken_WhenCredentialsAreValid()
    {
        var loginDto = new LoginDto
        {
            Email = "test@test.com",
            Password = "testPassword-1"
        };

        var response = await HttpClient.PostAsJsonAsync<LoginDto>("api/auth/login", loginDto);

        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<Response<AuthTokenDto>>();

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        token.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_GetError_WhenCredentialsAreInvalid()
    {
        var loginDto = new LoginDto
        {
            Email = "",
            Password = ""
        };

        var response = await HttpClient.PostAsJsonAsync<LoginDto>("api/auth/login", loginDto);

        var data = await response.Content.ReadFromJsonAsync<Response<object>>();
        var errors = data.Errors;

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors.Should().HaveCount(4);
        errors.Should().Contain(p => p.Contains("Login.Password"));
        errors.Should().Contain(p => p.Contains("Login.Email"));
    }

    [Fact]
    public async Task Register_GetToken_WithCredentialsAreValid()
    {
        var registerDto = new RegisterDto
        {
            Email = "test4@test.com",
            PublicNick = "Test1",
            FirstName = "Name1",
            LastName = "LastName1",
            Password = "Password-1",
            ConfirmPassword = "Password-1",
            DateOfBirth = new DateTime(1990, 5, 15)
        };

        var response = await HttpClient.PostAsJsonAsync("api/auth/register", registerDto);

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<object>>();

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        data.Errors.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Register_GetError_WithCredentialsAreInvalid()
    {
        var registerDto = new RegisterDto
        {
            Email = "",
            PublicNick = "PublicNick1",
            FirstName = "Name1",
            LastName = "LastName1",
            Password = "",
            ConfirmPassword = "",
            DateOfBirth = new DateTime(1990, 5, 15)
        };

        var response = await HttpClient.PostAsJsonAsync("api/auth/register", registerDto);

        var data = await response.Content.ReadFromJsonAsync<Response<object>>();
        var errors = data.Errors;

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors.Should().HaveCount(4);
        errors.Should().Contain(p => p.Contains("Register.Password"));
        errors.Should().Contain(p => p.Contains("Register.Email"));
    }

    [Fact]
    public async Task Refresh_GetToken_WhenTokenIsValid()
    {
        var loginDto = new LoginDto
        {
            Email = "test@test.com",
            Password = "testPassword-1"
        };

        var responseLogin = await HttpClient.PostAsJsonAsync<LoginDto>("api/auth/login", loginDto);
        responseLogin.EnsureSuccessStatusCode();
        var token = await responseLogin.Content.ReadFromJsonAsync<Response<AuthTokenDto>>();

        token.Data.Should().NotBeNull();
        var responseToken = await HttpClient.PostAsJsonAsync<string>("api/auth/refresh", token.Data.RefreshToken);
        var tokenRefreshed = await responseToken.Content.ReadFromJsonAsync<Response<AuthTokenDto>>();

        tokenRefreshed.Data.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Refresh_GetError_WhenTokenIsInvalid()
    {
        var responseToken = await HttpClient.PostAsJsonAsync<string>("api/auth/refresh", "bad_token");
        var tokenRefreshed = await responseToken.Content.ReadFromJsonAsync<Response<AuthTokenDto>>();
        var errors = tokenRefreshed?.Errors;

        tokenRefreshed.Data.Should().BeNull();
        tokenRefreshed.Errors.Should().NotBeNullOrEmpty();
        errors.Should().Contain(p => p.Contains("Token is expired"));
    }
}
