using System.Net.Http.Json;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace ChatShaker.ChatMauiApp.Services.Api;

public class KeyApiService : IKeyApiService
{
    private readonly HttpClient _httpClient;

    public KeyApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ShakerApiClient");
    }

    public async Task<Response<object>> UploadIdentity(UserKeyDataDto userKey)
    {
        var response = await _httpClient.PostAsJsonAsync("api/keys/save-identity", userKey);
        return await response.Content.ReadFromJsonAsync<Response<object>>();
    }

    public async Task<Response<List<UserKeyDataDto>>> GetPublicIdentities(List<Guid> userIds)
    {
        var queryParams = userIds.Select(id => new KeyValuePair<string?, string?>("userIds", id.ToString()));
        
        string uri = QueryHelpers.AddQueryString("api/keys/get-public-identities",  queryParams);
        var response = await _httpClient.GetFromJsonAsync<Response<List<UserKeyDataDto>>>(uri);
        
        return response;
    }

    public async Task<Response<string>> GetRoomKey(RoomKeyRequestInfoDto  requestInfo)
    {
        var response = await _httpClient.PostAsJsonAsync("api/keys/get-room-key", requestInfo);
        return await response.Content.ReadFromJsonAsync<Response<string>>();
    }

    public async Task<Response<object>> SaveRoomKey(RoomDto roomKeys)
    {
        var response = await _httpClient.PostAsJsonAsync("api/keys/create-room", roomKeys);
        return await response.Content.ReadFromJsonAsync<Response<object>>();
    }

    public async Task<Response<object>> InitializeRoom(RoomDto room)
    {
        var response = await _httpClient.PutAsJsonAsync("api/keys/initialize-room-key", room);
        return await response.Content.ReadFromJsonAsync<Response<object>>();
    }
    
    public async Task<Response<object>> SaveNewKeys(Guid publicId, IEnumerable<RoomKeyDataDto> keysData)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/keys/new-room-keys/{publicId}", keysData);
        return await response.Content.ReadFromJsonAsync<Response<object>>();
    }
}
