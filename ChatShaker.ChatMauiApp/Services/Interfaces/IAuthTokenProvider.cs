using ChatShaker.ChatMauiApp.Models.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Services.Interfaces
{
    public interface IAuthTokenProvider
    {
        Task<AuthData?> GetAuthToken();
        Task SetAuthToken(AuthData authData);
        Task ClearTokens();
    }
}
