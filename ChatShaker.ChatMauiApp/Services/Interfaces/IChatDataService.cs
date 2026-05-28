using ChatShaker.ChatMauiApp.Models.Dto;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IChatDataService
{
    Task<List<MessageDto>> GetDecryptedMessages(Guid roomPublicId, int pageIndex, int pageSize);
    Task<string> GetDecryptedMessage(Guid roomPublicId, string encryptedMessage, string nonce, long keyVersion);
    Task<(string, string)> SaveEncryptedMessage(Guid roomPublicId, string messageText, long keyVersion);
    Task<string> SaveEncryptedFile(Guid roomPublicId, byte[] fileBytes, string fileName, string contentType,  long keyVersion);
    Task<byte[]> GetDecryptedFile(Guid roomPublicId, Guid filePublicId, long keyVersion);
}
