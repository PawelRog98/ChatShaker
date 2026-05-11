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
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<object>>() ?? new Response<object> { Success = false, Message = "Empty response" };
        }
        return new Response<object> { Success = false, Message = $"Server returned {response.StatusCode}" };
    }

    public async Task<Response<List<UserKeyDataDto>>> GetPublicIdentities(List<Guid> userIds)
    {
        var queryParams = userIds.Select(id => new KeyValuePair<string?, string?>("userIds", id.ToString()));
        
        string uri = QueryHelpers.AddQueryString("api/keys/get-public-identities",  queryParams);
        var response = await _httpClient.GetAsync(uri);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<List<UserKeyDataDto>>>() ?? new Response<List<UserKeyDataDto>> { Success = false, Message = "Empty response" };
        }
        return new Response<List<UserKeyDataDto>> { Success = false, Message = $"Server returned {response.StatusCode}" };
    }

    public async Task<Response<string>> GetRoomKey(RoomKeyRequestInfoDto  requestInfo)
    {
        var response = await _httpClient.PostAsJsonAsync("api/keys/get-room-key", requestInfo);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<string>>() ?? new Response<string> { Success = false, Message = "Empty response" };
        }
        return new Response<string> { Success = false, Message = $"Server returned {response.StatusCode}" };
    }

    public async Task<Response<object>> SaveRoomKey(RoomDto roomKeys)
    {
        var response = await _httpClient.PostAsJsonAsync("api/keys/create-room", roomKeys);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<object>>() ?? new Response<object> { Success = false, Message = "Empty response" };
        }
        return new Response<object> { Success = false, Message = $"Server returned {response.StatusCode}" };
    }

    public async Task<Response<object>> InitializeRoom(RoomDto room)
    {
        var response = await _httpClient.PutAsJsonAsync("api/keys/initialize-room-key", room);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<object>>() ?? new Response<object> { Success = false, Message = "Empty response" };
        }
        return new Response<object> { Success = false, Message = $"Server returned {response.StatusCode}" };
    }
    
    public async Task<Response<object>> SaveNewKeys(Guid publicId, IEnumerable<RoomKeyDataDto> keysData)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/keys/new-room-keys/{publicId}", keysData);
        return await response.Content.ReadFromJsonAsync<Response<object>>();
    }
}
