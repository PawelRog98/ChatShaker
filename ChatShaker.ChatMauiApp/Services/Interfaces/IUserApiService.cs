using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Api;

public interface IUserApiService
{
    Task<Response<List<UserItemDto>>> GetFriends();
    Task<Response<List<UserKeyDataDto>>> GetParticipants(List<Guid> userIds);
}