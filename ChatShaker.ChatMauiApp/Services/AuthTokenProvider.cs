using ChatShaker.ChatMauiApp.Models.Local;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Services
{
    public class AuthTokenProvider : IAuthTokenProvider
    {
        private const string AccessTokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";
        private const string UserNick = "user_nick";

        public async Task<AuthData?> GetAuthToken()
        {
            var accessToken = await SecureStorage.GetAsync(AccessTokenKey);
            var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
            var userNick = await SecureStorage.GetAsync(UserNick);

            if (accessToken != null && refreshToken != null && userNick != null)
                return new AuthData
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserNick = userNick
                };

            return null;
        }

        public async Task SetAuthToken(AuthData authData)
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, authData.AccessToken);
            await SecureStorage.Default.SetAsync(RefreshTokenKey, authData.RefreshToken);
            await SecureStorage.Default.SetAsync(UserNick, authData.UserNick);
        }

        public async Task ClearTokens()
        {
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(UserNick);
        }
    }
}
