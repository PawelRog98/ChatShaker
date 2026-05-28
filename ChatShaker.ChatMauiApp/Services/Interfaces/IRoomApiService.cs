using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IRoomApiService
{
    Task<Response<List<ChatListItem>>> GetRooms();
    Task<Response<RoomDto>> GetRoom(Guid publicId);
    Task<Response<bool>> CheckIfRoomInitialized(Guid publicId);
    Task<Response<long>> GetKeyVersion(Guid publicId);
}
