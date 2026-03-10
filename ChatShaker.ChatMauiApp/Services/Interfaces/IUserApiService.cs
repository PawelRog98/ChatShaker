using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Api;

public interface IUserApiService
{
    Task<Response<List<UserInfoDto>>> GetFriends(string name);
}