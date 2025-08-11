using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Local;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IAuthTokenProvider _authTokenProvider;

        public AuthService(IApiService apiService, IAuthTokenProvider authTokenProvider)
        {
            _apiService = apiService;
            _authTokenProvider = authTokenProvider;
        }

        public async Task<Response<AuthData>> Login(LoginDto loginDto)
        {
            var response = await _apiService.Login(loginDto);

            if (response.Success)
            {
                await _authTokenProvider.SetAuthToken(response.Data);

                return response;
            }

            return response;
        }

        public async Task<Response<object>> Register(RegisterDto registerDto)
        {
            var response = await _apiService.Register(registerDto);

            return response;
        }

        public async Task<string?> GetAccessToken()
        {
            var tokens = await _authTokenProvider.GetAuthToken();
            if (tokens == null)
                return null;

            var ifExpired = IfTokenIsExpired(tokens.AccessToken);
            if(ifExpired)
            {
                var isRefreshed = await TryRefreshToken();
                if (!isRefreshed)
                    return null;

                tokens = await _authTokenProvider.GetAuthToken();
            }

            return tokens?.AccessToken;
        }

        public async Task<bool> TryRefreshToken()
        {
            var tokens = await _authTokenProvider.GetAuthToken();
            if (tokens == null)
                return false;

            var response = await _apiService.RefreshToken(tokens.RefreshToken);
            if(!response.Success) 
                return false;

            await _authTokenProvider.SetAuthToken(response.Data);
            return true;
        }

        private bool IfTokenIsExpired(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo < DateTime.UtcNow;
        }
    }

}
