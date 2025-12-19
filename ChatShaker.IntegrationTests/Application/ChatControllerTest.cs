using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.IntegrationTests.Fixtures;
using ChatShaker.Application.Chats.CreateChatRoom.Commands;
using FluentAssertions;
using ChatShaker.Domain.Entities;
using ChatShaker.Application.Chats.Commands.AddMemberToRoom;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ChatShaker.IntegrationTests.Application;

[Collection("IntegrationTests")]
public class ChatControllerTest : IntegrationTestBase
{
    public ChatControllerTest(IntegrationTestsWebAppFactory factory) : base(factory){}

    [Fact]
    public async Task CreateChat_GetOkReturn_WhenValid()
    {
        await GetTokenForAuth();

        var users = await GetSeededUsersByNumber(2);

        var usersDto = new List<UserEncryptionDto>();

        foreach(var userData in users)
        {
            usersDto.Add(new UserEncryptionDto
            {
                PublicId = userData.PublicId,
                EncryptedUserKey = Guid.NewGuid().ToString(),
                isHost = users.First() == userData ? true : false 
            });
        }

        var createChatRoomDto = new CreateChatRoomDto
        {
            Name = "test1",
            CreatedAtUtc = DateTime.UtcNow,
            Users = usersDto
        };

        var response = await HttpClient.PostAsJsonAsync("api/chatroom/create-room", createChatRoomDto);

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<object>>();

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        data.Errors.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task AddUser_GetOkReturn_WhenValid()
    {
        await GetTokenForAuth();

        var users = await GetSeededUsersByNumber(2);

        var host = users.First();
        var userToAdd = users.Last();

        var usersDto = new List<UserEncryptionDto>();
        usersDto.Add(new UserEncryptionDto
            {
                PublicId = host.PublicId,
                EncryptedUserKey = Guid.NewGuid().ToString(),
                isHost = true
            });

        var createChatRoomDto = new CreateChatRoomDto
        {
            Name = "test2",
            CreatedAtUtc = DateTime.UtcNow,
            Users = usersDto
        };

        var response = await HttpClient.PostAsJsonAsync("api/chatroom/create-room", createChatRoomDto);

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<Guid>>();

        var roomPublicId = data.Data;

        var dtoMember = new AddMemberToRoomDto
        {
            RoomPublicId = roomPublicId,
            UserToAddPublicId = userToAdd.PublicId,
            EncryptedKey = "test_encrypted_key_111"
        };

        var responseRoom = await HttpClient.PostAsJsonAsync("api/chatroom/add-member", dtoMember);

        response.EnsureSuccessStatusCode();
        var room = await Context.ChatRooms
            .FirstAsync(r => r.PublicId == roomPublicId);

        var membership = await Context.ChatRoomMemberships
            .FirstOrDefaultAsync(x=>x.ChatRoomId == room.Id && 
            x.UserId == userToAdd.Id);

        var blob = await Context.ChatRoomKeyBlobs
            .FirstOrDefaultAsync(x=>x.ChatRoomId == room.Id &&
            x.UserId == userToAdd.Id);

        membership.Should().NotBeNull();
        blob.Should().NotBeNull();

    }

    [Fact]
    public async Task AddUser_GetError_WhenRommNotExists()
    {
        await GetTokenForAuth();
        var users = await GetSeededUsersByNumber(2);

        var host = users.First();
        var userToAdd = users.Last();

        var dtoMember = new AddMemberToRoomDto
        {
            RoomPublicId = Guid.NewGuid(),
            UserToAddPublicId = userToAdd.PublicId,
            EncryptedKey = "test_encrypted_key_222"
        };

        var response = await HttpClient.PostAsJsonAsync("api/chatroom/add-member", dtoMember);
        var data = await response.Content.ReadFromJsonAsync<Response<object>>();
        var errors = data.Errors;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errors.Should().NotBeNull();
        errors.Should().Contain(p=>p.Contains("Room doesn't exists"));
    }

    [Fact]
    public async Task AddUser_GetError_WhenUserNotExists()
    {
        await GetTokenForAuth();

        var users = await GetSeededUsersByNumber(1);
        var host = users.First();

        var usersDto = new List<UserEncryptionDto>();
        usersDto.Add(new UserEncryptionDto
            {
                PublicId = host.PublicId,
                EncryptedUserKey = Guid.NewGuid().ToString(),
                isHost = true
            });

        var createChatRoomDto = new CreateChatRoomDto
        {
            Name = "test2",
            CreatedAtUtc = DateTime.UtcNow,
            Users = usersDto
        };

        var response = await HttpClient.PostAsJsonAsync("api/chatroom/create-room", createChatRoomDto);

        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Response<Guid>>();

        var roomPublicId = data.Data;

        var dtoMember = new AddMemberToRoomDto
        {
            RoomPublicId = roomPublicId,
            UserToAddPublicId = Guid.NewGuid(),
            EncryptedKey = "test_encrypted_key_333"
        };

        var responseMember = await HttpClient.PostAsJsonAsync("api/chatroom/add-member", dtoMember);
        var dataMember = await responseMember.Content.ReadFromJsonAsync<Response<object>>();
        var errors = dataMember.Errors;

        responseMember.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errors.Should().NotBeNull();
        errors.Should().Contain(p=>p.Contains("User doesn't exists"));
    }
}
