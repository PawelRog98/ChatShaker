using System.Net.Http.Json;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace ChatShaker.ChatMauiApp.Services.Api;

public class FriendshipApiService : IFriendshipApiService
{
    private readonly HttpClient _httpClient;
    
    public FriendshipApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ShakerApiClient");
    }
    
    public async Task<Response<object>> SendInvitation(string invitationCode)
    {
        var queryParams = new Dictionary<string, string>();
        queryParams.Add("invitationCode", invitationCode);
        string uri = QueryHelpers.AddQueryString("api/friendship/send",  queryParams);
        
        var response = await _httpClient.PostAsync(uri, null);
        return await response.Content.ReadFromJsonAsync<Response<object>>();
    }

    public async Task<Response<List<SentInvitationDto>>> GetSentInvitations()
    {
        return await _httpClient.GetFromJsonAsync<Response<List<SentInvitationDto>>>("api/friendship/get-sent");
    }
}