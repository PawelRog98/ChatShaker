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
    public interface IApiService
    {
        Task<Response<AuthData>> Login(LoginDto loginDto);
        Task<Response<object>> Register(RegisterDto registerDto);
        Task<Response<AuthData>> RefreshToken(string refreshToken);
    }
}
