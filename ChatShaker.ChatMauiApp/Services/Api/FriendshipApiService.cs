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

    public async Task<Response<List<InvitationDto>>> GetSentInvitations()
    {
        return await _httpClient.GetFromJsonAsync<Response<List<InvitationDto>>>("api/friendship/get-sent");
    }

    public async Task<Response<List<InvitationDto>>> GetRecievedInvitations()
    {
        return await _httpClient.GetFromJsonAsync<Response<List<InvitationDto>>>("api/friendship/get-recieved");
    }

    public async Task<Response<object>> RespondToInvitation(Guid invitationId, bool accept)
    {
        var decision = new AcceptanceDecisionDto
        {
            InvitationRequestId = invitationId,
            IsAccepted = accept
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/friendship/accept-invitation", decision);
        return await response.Content.ReadFromJsonAsync<Response<object>>();
    }
}