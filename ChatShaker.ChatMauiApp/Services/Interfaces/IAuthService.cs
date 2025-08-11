using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Models.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Response<AuthData>> Login(LoginDto loginDto);
        Task<Response<object>> Register(RegisterDto registerDto);
        Task<string?> GetAccessToken();
        Task<bool> TryRefreshToken();
    }
}
