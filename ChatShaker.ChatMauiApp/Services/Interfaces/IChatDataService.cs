using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IChatDataService
{
    Task<List<MessageDto>> GetDecryptedMessages(Guid roomPublicId, int pageIndex, int pageSize);
    Task<(string, string)> SaveEncryptedMessage(Guid roomPublicId, string messageText);
}
