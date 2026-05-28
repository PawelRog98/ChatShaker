using System.Net.Http.Json;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Microsoft.AspNetCore.WebUtilities;

namespace ChatShaker.ChatMauiApp.Services.Api;

public class MessagesApiService : IMessagesApiService
{
    private readonly HttpClient _httpClient;

    public MessagesApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ShakerApiClient");
    }

    public async Task<Response<List<MessageDto>>> GetMessages(Guid roomPublicId, int pageIndex, int pageSize)
    {
        var query = new Dictionary<string, string>
        {
            {"pageIndex", pageIndex.ToString()},
            {"pageSize", pageSize.ToString()} 
        };

        var uri = QueryHelpers.AddQueryString($"api/chatroom/{roomPublicId}/messages", query);
        var items = await _httpClient.GetFromJsonAsync<Response<List<MessageDto>>>(uri);

        return items;
    }
}
