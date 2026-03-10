using ChatShaker.ChatMauiApp.Helpers;
using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IMessagesApiService
{
    Task<Response<List<MessageDto>>> GetMessages(Guid roomPublicId, int pageIndex, int pageSize);
}
