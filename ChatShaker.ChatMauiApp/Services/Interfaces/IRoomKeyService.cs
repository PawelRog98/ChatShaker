using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IRoomKeyService
{
    Task<byte[]> GetRoomKey(Guid roomPublicId, long? version = null);
    Task GenerateAndSaveRoomKey(IEnumerable<UserKeyDataDto> userKeys, string name);
    Task InitializeRoomKeyForExistingRoom(IEnumerable<UserKeyDataDto> userKeys, Guid roomPublicId);
    Task RotateRoomKey(Guid roomPublicId, IEnumerable<UserKeyDataDto> userKeys);
    Task SyncAndRotateKey(Guid roomPublicId);
    Task ShareKeyDataWithUser(Guid roomPublicId, long version, IEnumerable<UserKeyDataDto> userKeys);
}