using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IKeyApiService
{
    Task<Response<object>> UploadIdentity(UserKeyDataDto userKey);
    Task<Response<List<UserKeyDataDto>>> GetPublicIdentities(List<Guid> userIds);
    Task<Response<string>> GetRoomKey(Guid roomPublicId);
    Task<Response<object>> SaveRoomKey(RoomDto roomKeys);
}
