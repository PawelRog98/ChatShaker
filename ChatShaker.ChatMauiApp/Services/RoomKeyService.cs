using System.Security.Cryptography;
using System.Text;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services;

public class RoomKeyService : IRoomKeyService
{
    private const string RoomKeyKey = "room_key_";
    
    private readonly IKeyApiService _keyApiService;
    private readonly ICryptoService _cryptoService;

    public RoomKeyService(IKeyApiService keyApiService,  ICryptoService cryptoService)
    {
        _keyApiService = keyApiService;
        _cryptoService = cryptoService;
    }
    
    public async Task<byte[]> GetRoomKey(Guid roomPublicId)
    {
        var encryptedKey = await _keyApiService.GetRoomKey(roomPublicId);
        var privateKeyData = await SecureStorage.GetAsync(RoomKeyKey+roomPublicId.ToString());
        
        var privateKey = Encoding.UTF8.GetBytes(privateKeyData);
        
        var roomKey = await _cryptoService.DecryptRoomKey(encryptedKey.Data, privateKey);
        
        return roomKey;
    }  

    public async Task GenerateAndSaveRoomKey(IEnumerable<UserInfoDto> userKeys, string name)
    {
        var roomKey = RandomNumberGenerator.GetBytes(32);
        
        var publicKeys = new List<RoomKeyDataDto>();
        var room = new RoomDto
        {
            Name = name,
            CreateDateUtc =  DateTime.UtcNow
        };

        foreach(var user in userKeys)
        {
            var publicKeyBytes = Encoding.UTF8.GetBytes(user.PublicKey);
            var encryptedKey = await _cryptoService.EncryptRoomKey(roomKey, publicKeyBytes);

            var dataToSave = new RoomKeyDataDto
            {
                UserId =  user.PublicId,
                EncryptedRoomKey = encryptedKey
            };
            
            publicKeys.Add(dataToSave);
        }
        
        room.Keys = publicKeys;
        await _keyApiService.SaveRoomKey(room);
    }
}