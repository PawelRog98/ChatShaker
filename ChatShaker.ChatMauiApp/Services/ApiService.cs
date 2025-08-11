using ChatShaker.ChatMauiApp.Services.Interfaces;
using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("ShakerApiClient");
        }

        public async Task<Response<AuthData>> Login(LoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);
            return await response.Content.ReadFromJsonAsync<Response<AuthData>>();
        }

        public async Task<Response<object>> Register(RegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerDto);
            var test = await response.Content.ReadAsStringAsync();
            return await response.Content.ReadFromJsonAsync<Response<object>>();
        }

        public async Task<Response<AuthData>> RefreshToken(string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", refreshToken);
            return await response.Content.ReadFromJsonAsync<Response<AuthData>>();
        }
    }
}
