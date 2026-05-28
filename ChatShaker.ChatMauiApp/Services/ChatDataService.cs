using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services;

public class ChatDataService : IChatDataService
{
    private readonly IMessagesApiService _messagesApiService;
    private readonly ICryptoService _cryptoService;
    private readonly IRoomKeyService _roomKeyService;
    private readonly IFileApiService _fileApiService;

    public ChatDataService(IMessagesApiService messagesApiService, 
        ICryptoService cryptoService, 
        IRoomKeyService roomKeyService,
        IFileApiService fileApiService)
    {
        _messagesApiService = messagesApiService;
        _cryptoService = cryptoService;
        _roomKeyService = roomKeyService;
        _fileApiService = fileApiService;
    }
    public async Task<List<MessageDto>> GetDecryptedMessages(Guid roomPublicId, int pageIndex, int pageSize)
    {

        var items = await _messagesApiService.GetMessages(roomPublicId, pageIndex, pageSize);
        
        if (items == null)
            throw new Exception("Failed to get response from server");

        if (!items.Success)
            throw new Exception(items.Message ?? "Failed to load messages");

        if (items.Data == null || !items.Data.Any())
            return new List<MessageDto>();

        var decryptionTasks = items.Data.Select(async item =>
        {
            var roomKey = await _roomKeyService.GetRoomKey(roomPublicId, item.KeyVersion);
            item.CipherText = await _cryptoService.DecryptMessage(roomKey, item.CipherText, item.Nonce);
        });

        await Task.WhenAll(decryptionTasks);
        
        return items.Data;
    }

    public async Task<string> GetDecryptedMessage(Guid roomPublicId, string encryptedMessage, string nonce, long keyVersion)
    {
        var roomKey = await _roomKeyService.GetRoomKey(roomPublicId, keyVersion);
        
        return await _cryptoService.DecryptMessage(roomKey, encryptedMessage, nonce);
    }

    public async Task<(string, string)> SaveEncryptedMessage(Guid roomPublicId, string messageText, long keyVersion)
    {
        var roomKey = await _roomKeyService.GetRoomKey(roomPublicId, keyVersion);
        var messageData = await _cryptoService.EncryptMessage(roomKey, messageText);

        return messageData;
    }

    public async Task<string> SaveEncryptedFile(Guid roomPublicId, byte[] fileBytes, string fileName, string contentType, long keyVersion)
    {
        var roomKey = await _roomKeyService.GetRoomKey(roomPublicId, keyVersion);
        var (cipherBytes, nonce) = await _cryptoService.EncryptBytes(roomKey, fileBytes);
        
        var combined = new byte[nonce.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, nonce.Length, cipherBytes.Length);

        var response = await _fileApiService.UploadFile(combined, fileName, contentType, roomPublicId);
        
        if (response.Success)
        {
            return response.Data;
        }

        throw new Exception(response.Message);
    }

    public async Task<byte[]> GetDecryptedFile(Guid roomPublicId, Guid filePublicId, long keyVersion)
    {
        var fileStream = await _fileApiService.DownloadFile(filePublicId);

        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);

        var combined = ms.ToArray();
        var nonce = new byte[12];
        var cipher = new byte[combined.Length - 12];
        
        Buffer.BlockCopy(combined, 0, nonce, 0, 12);
        Buffer.BlockCopy(combined, 12, cipher, 0, cipher.Length);

        var roomKey = await _roomKeyService.GetRoomKey(roomPublicId, keyVersion);
        return await _cryptoService.DecryptBytes(roomKey, cipher, nonce);
    }
}
