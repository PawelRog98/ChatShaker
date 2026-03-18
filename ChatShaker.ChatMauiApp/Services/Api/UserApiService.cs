using System.Net.Http.Json;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using Microsoft.AspNetCore.WebUtilities;

namespace ChatShaker.ChatMauiApp.Services.Api;

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;

    public UserApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ShakerApiClient");
    }
    
    public async Task<Response<List<UserItemDto>>> GetFriends()
    {
        return await _httpClient.GetFromJsonAsync<Response<List<UserItemDto>>> ("api/users/get-friends");
    }

    public async Task<Response<List<UserKeyDataDto>>> GetParticipants(List<Guid> userIds)
    {
        var queryParams = new Dictionary<string, string>();
        for (int i = 0; i < userIds.Count; i++)
        {
            queryParams.Add($"userId{i}", userIds[i].ToString());
        }
        
        string uri = QueryHelpers.AddQueryString("api/users/get-public-identities",  queryParams);
        var response = await _httpClient.GetFromJsonAsync<Response<List<UserKeyDataDto>>>(uri);
        
        return response;
    }
}