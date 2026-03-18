using System.Security.Cryptography;
using System.Text;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services;

public class RoomKeyService : IRoomKeyService
{
    private const string RoomKeyKey = "room_key";
    private const string PrivateIdentityKeyKey = "identity_private_key";
    
    private readonly IKeyApiService _keyApiService;
    private readonly ICryptoService _cryptoService;
    private readonly IRoomApiService _roomApiService;

    public RoomKeyService(IKeyApiService keyApiService,  ICryptoService cryptoService,  IRoomApiService roomApiService)
    {
        _keyApiService = keyApiService;
        _cryptoService = cryptoService;
        _roomApiService = roomApiService;
    }
    
    private byte[] GenerateRoomKey(int size=32)
        => RandomNumberGenerator.GetBytes(size);
    
    public async Task<byte[]> GetRoomKey(Guid roomPublicId)
    {
        var roomVersion = await _roomApiService.GetKeyVersion(roomPublicId);
        
        string roomKeyData = string.Empty;
        byte[] roomKey;
        roomKeyData = await SecureStorage.GetAsync($"{RoomKeyKey}_{roomPublicId}_{roomVersion.Data}");

        if (string.IsNullOrEmpty(roomKeyData))
        {
            var roomKeyEncryptedData = await _keyApiService.GetRoomKey(roomPublicId, roomVersion.Data);
            var privateKeyData = await SecureStorage.GetAsync($"{PrivateIdentityKeyKey}");
            
            var privateKey = Encoding.UTF8.GetBytes(privateKeyData);
            
            roomKey = await _cryptoService.DecryptRoomKey(roomKeyEncryptedData.Data, privateKey);
            await SecureStorage.SetAsync($"{RoomKeyKey}_{roomPublicId}_{roomVersion.Data}", roomKey.ToString());
        }
        else
        {
            roomKey = Encoding.UTF8.GetBytes(roomKeyData);
        }
        
        return roomKey;
    }  

    public async Task GenerateAndSaveRoomKey(IEnumerable<UserKeyDataDto> userKeys, string name)
    {
        var roomKey = GenerateRoomKey();
        
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
                UserId =  user.PublicUserId.Value,
                EncryptedRoomKey = encryptedKey,
                Version = 1
            };
            
            publicKeys.Add(dataToSave);
        }
        
        room.Keys = publicKeys;
        await _keyApiService.SaveRoomKey(room);
    }

    public async Task InitializeRoomKeyForExistingRoom(IEnumerable<UserKeyDataDto> userKeys, Guid roomPublicId)
    {
        var roomKey = GenerateRoomKey();
        
        var publicKeys = new List<RoomKeyDataDto>();
        var room = new RoomDto
        {
            PublicId = roomPublicId,
            Keys = publicKeys
        };

        foreach (var user in userKeys)
        {
            var publicKeyBytes = Encoding.UTF8.GetBytes(user.PublicKey);
            
            var encryptedKey = await _cryptoService.EncryptRoomKey(roomKey, publicKeyBytes);
            
            var dataToSave = new RoomKeyDataDto
            {
                UserId =  user.PublicUserId.Value,
                EncryptedRoomKey = encryptedKey,
                Version = 1,
                DeviceId =  user.DeviceId
            };
            
            publicKeys.Add(dataToSave);
        }
        room.Keys = publicKeys;
        
        await _keyApiService.InitializeRoom(room);
    }

    public async Task RotateRoomKey(Guid roomPublicId, IEnumerable<UserKeyDataDto> userKeys)
    {
        var roomKey = GenerateRoomKey();
        
        var roomVersion = await _roomApiService.GetKeyVersion(roomPublicId);
        var nextVersion = roomVersion.Data + 1;
        
        var publicKeys = new List<RoomKeyDataDto>();
        
        foreach (var user in userKeys)
        {
            var publicKeyBytes = Encoding.UTF8.GetBytes(user.PublicKey);
            
            var encryptedKey = await _cryptoService.EncryptRoomKey(roomKey, publicKeyBytes);
            
            var dataToSave = new RoomKeyDataDto
            {
                UserId =  user.PublicUserId.Value,
                EncryptedRoomKey = encryptedKey,
                Version = nextVersion,
                DeviceId = user.DeviceId
            };
            
            publicKeys.Add(dataToSave);
        }
        
        //await SecureStorage.SetAsync($"{RoomKeyKey}_{roomPublicId}_{nextVersion}", roomKey.ToString());
        await _keyApiService.SaveNewRotation(roomPublicId, publicKeys);
    }

    public async Task ShareKeyDataWithUser(Guid roomPublicId, long version, IEnumerable<UserKeyDataDto> userKeys)
    {
        var roomKeyData = await SecureStorage.GetAsync($"{RoomKeyKey}_{roomPublicId}_{version}");
        
        var roomKey = Encoding.UTF8.GetBytes(roomKeyData);
        
        var encryptedKeys =  new List<RoomKeyDataDto>();

        foreach (var device in userKeys)
        {
            var newUserKey = Encoding.UTF8.GetBytes(device.PublicKey);
            var encryptedRoomKey = await _cryptoService.EncryptRoomKey(roomKey, newUserKey);
            
            encryptedKeys.Add(new RoomKeyDataDto
            {
                UserId =  device.PublicUserId.Value,
                EncryptedRoomKey = encryptedRoomKey,
                Version = version,
                IsHost = false,
                DeviceId = device.DeviceId
            });
        }
        
        await _keyApiService.SaveOtherUserRoomKey(roomPublicId, encryptedKeys);
    }
}