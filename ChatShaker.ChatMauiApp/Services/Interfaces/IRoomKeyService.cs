using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IRoomKeyService
{
    Task<byte[]> GetRoomKey(Guid roomPublicId);
    Task GenerateAndSaveRoomKey(IEnumerable<UserInfoDto> userKeys, string name);
}