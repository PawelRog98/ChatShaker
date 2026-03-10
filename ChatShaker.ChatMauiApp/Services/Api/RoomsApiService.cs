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
        try
        {
            var items = await _httpClient.GetFromJsonAsync<Response<List<ChatListItem>>>($"api/chatroom/get-all");

            return items;
        }
        catch (Exception e)
        {
            throw;
        }
    }
}
