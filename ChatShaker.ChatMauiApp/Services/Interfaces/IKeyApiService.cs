using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IKeyApiService
{
    Task<Response<object>> UploadIdentity(UserKeyDataDto userKey);
    Task<Response<List<UserKeyDataDto>>> GetPublicIdentities(List<Guid> userIds);
    Task<Response<string>> GetRoomKey(RoomKeyRequestInfoDto requestInfo);
    Task<Response<object>> SaveRoomKey(CreateChatRoomDto roomKeys);
    Task<Response<object>> InitializeRoom(ChatRoomDto room);
    Task<Response<object>> SaveNewKeys(Guid publicId, IEnumerable<RotationDto> keysData);
}
