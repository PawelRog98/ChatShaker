using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IRoomApiService
{
    Task<Response<List<ChatListItem>>> GetRooms();
}
