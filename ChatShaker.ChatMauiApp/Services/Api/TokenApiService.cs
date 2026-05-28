using System.Net.Http.Json;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services.Api;

public class TokenApiService : ITokenApiService
{
    private readonly HttpClient _httpClient;

    public TokenApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ShakerApiClient");
    }

    public async Task<Response<object>> ActivateAccount(string code, string email)
    {
        var data = new ConfirmAccountDto
        {
            Token = code,
            Email = email
        };
        
        var response = await _httpClient.PutAsJsonAsync("api/tokens/activate-account",  data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<object>>() ?? new Response<object> { Success = false, Message = "Empty response" };
        }
        
        return new Response<object> { Success = false, Message = $"Server returned {response.StatusCode}" };
    }

    public async Task<Response<object>> CreateNewActivationToken(string email)
    {
        var data = new CreateNewTokenDto
        {
            Email = email
        };
        
        var response = await _httpClient.PostAsJsonAsync("api/tokens/create-new-activation-token", data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Response<object>>() ?? new Response<object> { Success = false, Message = "Empty response" };
        }
        
        return new Response<object> { Success = false, Message = $"Server returned {response.StatusCode}" };
    }
}