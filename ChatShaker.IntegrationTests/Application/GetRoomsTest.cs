using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.IntegrationTests.Fixtures;
using ChatShaker.Application.Chats.CreateChatRoom.Commands;
using FluentAssertions;
using ChatShaker.Application.Chats.Queries.GetUserRooms;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class GetRoomsTest : IntegrationTestBase
{
    public GetRoomsTest(IntegrationTestsWebAppFactory factory) : base(factory){}

    [Fact]
    public async Task GetRooms_ReturnsOk_WhenUserHasRooms()
    {
        // Arrange
        await GetTokenForAuth();
        var user = await Context.Users.FirstAsync(x => x.Email == "test@test.com");

        var usersDto = new List<UserEncryptionDto>
        {
            new UserEncryptionDto
            {
                UserId = user.PublicId,
                EncryptedUserKey = Guid.NewGuid().ToString(),
                IsHost = true,
                DeviceId = "test-device"
            }
        };

        var createChatRoomDto = new CreateChatRoomDto
        {
            Name = "Test Room",
            CreatedAtUtc = DateTime.UtcNow,
            Keys = usersDto
        };

        var createResponse = await HttpClient.PostAsJsonAsync("api/keys/create-room", createChatRoomDto);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await HttpClient.GetAsync("api/chatroom/get-all");

        // Assert
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<List<ChatListItemDto>>>();
        data.Data.Should().NotBeNull();
        data.Data.Should().Contain(x => x.Name == "Test Room");
    }

    [Fact]
    public async Task GetRooms_ReturnsOk_WhenUserHasNoRooms()
    {
        // Arrange
        await GetTokenForAuth();

        // Act
        var response = await HttpClient.GetAsync("api/chatroom/get-all");

        // Assert
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<List<ChatListItemDto>>>();
        data.Data.Should().NotBeNull();
    }
}
