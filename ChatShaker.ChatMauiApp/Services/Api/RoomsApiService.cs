using System.Net.Http.Json;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace ChatShaker.ChatMauiApp.Services.Api;

public class RoomsApiService : IRoomApiService
{
    private readonly HttpClient _httpClient;

    public RoomsApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ShakerApiClient");
    }

    public async Task<Response<List<ChatListItem>>> GetRooms()
    {
        var items = await _httpClient.GetFromJsonAsync<Response<List<ChatListItem>>>($"api/chatroom/get-all");

        return items;
    }

    public async Task<Response<RoomDto>> GetRoom(Guid publicId)
    {
        var room = await _httpClient.GetFromJsonAsync<Response<RoomDto>>($"api/chatroom/{publicId}");
        return room;
    }

    public async Task<Response<bool>> CheckIfRoomInitialized(Guid publicId)
    {
        return await _httpClient.GetFromJsonAsync<Response<bool>>($"api/chatroom/initialization-status/{publicId}");
    }

    public async Task<Response<long>> GetKeyVersion(Guid publicId)
    {
        return await _httpClient.GetFromJsonAsync<Response<long>>($"api/chatroom/key-version/{publicId}");
    }
}
