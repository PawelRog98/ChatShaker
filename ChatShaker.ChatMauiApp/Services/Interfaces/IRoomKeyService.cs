using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IRoomKeyService
{
    Task<byte[]> GetRoomKey(Guid roomPublicId);
    Task GenerateAndSaveRoomKey(IEnumerable<UserKeyDataDto> userKeys, string name);
    Task InitializeRoomKeyForExistingRoom(IEnumerable<UserKeyDataDto> userKeys, Guid roomPublicId);
    Task RotateRoomKey(Guid roomPublicId, IEnumerable<UserKeyDataDto> userKeys);
    Task ShareKeyDataWithUser(Guid roomPublicId, long version, IEnumerable<UserKeyDataDto> userKeys);
}