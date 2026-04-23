using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services;

public class ChatDataService : IChatDataService
{
    private readonly IMessagesApiService _messagesApiService;
    private readonly ICryptoService _cryptoService;
    private readonly IRoomKeyService _roomKeyService;

    public ChatDataService(IMessagesApiService messagesApiService, ICryptoService cryptoService,  IRoomKeyService roomKeyService)
    {
        _messagesApiService = messagesApiService;
        _cryptoService = cryptoService;
        _roomKeyService = roomKeyService;
    }
    public async Task<List<MessageDto>> GetDecryptedMessages(Guid roomPublicId, int pageIndex, int pageSize)
    {

        var items = await _messagesApiService.GetMessages(roomPublicId, pageIndex, pageSize);
        var roomKey = await _roomKeyService.GetRoomKey(roomPublicId);

        var decryptionTasks = items.Data.Select(async item =>
        {
            item.CipherText = await _cryptoService.DecryptMessage(roomKey, item.CipherText, item.Nonce);
        });

        await Task.WhenAll(decryptionTasks);
        
        return items.Data;
    }

    public async Task<(string, string)> SaveEncryptedMessage(Guid roomPublicId, string messageText)
    {
        var roomKey = await _roomKeyService.GetRoomKey(roomPublicId);
        var messageData = await _cryptoService.EncryptMessage(roomKey, messageText);

        return messageData;
    }
}
