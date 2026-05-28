using System.Security.Cryptography;
using System.Text;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services;

public class RoomKeyService : IRoomKeyService
{
    private const string RoomKeyKey = "room_key";
    private const string PrivateIdentityKeyKey = "identity_private_key_";
    
    private readonly IKeyApiService _keyApiService;
    private readonly ICryptoService _cryptoService;
    private readonly IRoomApiService _roomApiService;
    private readonly IAuthTokenProvider _authTokenProvider;

    public RoomKeyService(IKeyApiService keyApiService,  ICryptoService cryptoService,  IRoomApiService roomApiService, IAuthTokenProvider authTokenProvider)
    {
        _keyApiService = keyApiService;
        _cryptoService = cryptoService;
        _roomApiService = roomApiService;
        _authTokenProvider = authTokenProvider;
    }
    
    private byte[] GenerateRoomKey(int size=32)
        => RandomNumberGenerator.GetBytes(size);
    
    public async Task<byte[]> GetRoomKey(Guid roomPublicId, long? version = null)
    {
        try
        {
            long roomKeyVersion;
            if (version == null)
            {
                var roomVersion = await _roomApiService.GetKeyVersion(roomPublicId);
                if (roomVersion == null || !roomVersion.Success)
                {
                    throw new Exception($"Failed to get room key version: {roomVersion?.Message}");
                }
                
                roomKeyVersion = roomVersion.Data;
            }
            else
            {
                roomKeyVersion = version.Value;
            }
            

            var deviceId = Preferences.Default.Get("UniqueDeviceId", string.Empty);
            var fullDeviceId = $"{DeviceInfo.Current.Model}_{deviceId}";

            string roomKeyData = string.Empty;
            byte[] roomKey;
            roomKeyData = await SecureStorage.GetAsync($"{RoomKeyKey}_{roomPublicId}_{roomKeyVersion}");

            if (string.IsNullOrEmpty(roomKeyData))
            {
                var roomKeyInfoRequest = new RoomKeyRequestInfoDto
                {
                    Version = roomKeyVersion,
                    DeviceId = fullDeviceId,
                    PublicId = roomPublicId
                };

                var roomKeyEncryptedData = await _keyApiService.GetRoomKey(roomKeyInfoRequest);
                if (roomKeyEncryptedData == null || !roomKeyEncryptedData.Success || string.IsNullOrEmpty(roomKeyEncryptedData.Data))
                {
                    throw new Exception($"Failed to retrieve encrypted room key: {roomKeyEncryptedData?.Message}");
                }

                if (roomKeyEncryptedData.Data.Equals("Placeholder", StringComparison.OrdinalIgnoreCase))
                {
                    return Array.Empty<byte>();
                }
                
                var userData = await _authTokenProvider.GetAuthToken();

                var privateKeyData = await SecureStorage.GetAsync(PrivateIdentityKeyKey+userData.UserId);
                if (string.IsNullOrEmpty(privateKeyData))
                {
                    throw new InvalidOperationException("Private identity key not found");
                }

                var privateKey = Convert.FromBase64String(privateKeyData);

                roomKey = await _cryptoService.DecryptRoomKey(roomKeyEncryptedData.Data, privateKey);
                await SecureStorage.SetAsync($"{RoomKeyKey}_{roomPublicId}_{roomKeyVersion}", Convert.ToBase64String(roomKey));
            }
            else
            {
                roomKey = Convert.FromBase64String(roomKeyData);
            }

            return roomKey;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in GetRoomKey for room {roomPublicId}: {ex.Message}", ex);
        }
    }  

    public async Task GenerateAndSaveRoomKey(IEnumerable<UserKeyDataDto> userKeys, string name)
    {
        var roomKey = GenerateRoomKey();
        
        var publicKeys = new List<UserEncryptionDto>();
        var room = new CreateChatRoomDto
        {
            Name = name,
            CreatedAtUtc =  DateTime.UtcNow,
            Keys = publicKeys
        };

        foreach(var user in userKeys)
        {
            var publicKeyBytes = Convert.FromBase64String(user.PublicKey);
            var encryptedKey = await _cryptoService.EncryptRoomKey(roomKey, publicKeyBytes);

            var dataToSave = new UserEncryptionDto
            {
                UserId =  user.PublicUserId.Value,
                EncryptedUserKey = encryptedKey,
                Version = 1,
                DeviceId = user.DeviceId
            };
            
            publicKeys.Add(dataToSave);
        }
        
        await _keyApiService.SaveRoomKey(room);
    }

    public async Task InitializeRoomKeyForExistingRoom(IEnumerable<UserKeyDataDto> userKeys, Guid roomPublicId)
    {
        var roomKey = GenerateRoomKey();
        
        var publicKeys = new List<RoomKeyBlobDto>();
        var room = new ChatRoomDto
        {
            ChatRoomPublicId = roomPublicId,
            ChatRoomKeyBlobDtos = publicKeys
        };

        foreach (var user in userKeys)
        {
            var publicKeyBytes = Convert.FromBase64String(user.PublicKey);
            
            var encryptedKey = await _cryptoService.EncryptRoomKey(roomKey, publicKeyBytes);
            
            var dataToSave = new RoomKeyBlobDto
            {
                UserPublicId =  user.PublicUserId.Value,
                EncryptedRoomKey = encryptedKey,
                DeviceId =  user.DeviceId,
                CreatedAtUtc = DateTime.UtcNow
            };
            
            publicKeys.Add(dataToSave);
        }
        
        await _keyApiService.InitializeRoom(room);
    }

    public async Task RotateRoomKey(Guid roomPublicId, IEnumerable<UserKeyDataDto> userKeys)
    {
        var roomKey = GenerateRoomKey();

        var roomVersion = await _roomApiService.GetKeyVersion(roomPublicId);
        var nextVersion = roomVersion.Data + 1;

        await SecureStorage.SetAsync($"{RoomKeyKey}_{roomPublicId}_{nextVersion}", Convert.ToBase64String(roomKey));

        var publicKeys = new List<RotationDto>();

        foreach (var user in userKeys)
        {
            var publicKeyBytes = Convert.FromBase64String(user.PublicKey);

            var encryptedKey = await _cryptoService.EncryptRoomKey(roomKey, publicKeyBytes);

            var dataToSave = new RotationDto
            {
                UserId =  user.PublicUserId.Value,
                EncryptedRoomKey = encryptedKey,
                Version = nextVersion,
                DeviceId =  user.DeviceId
            };

            publicKeys.Add(dataToSave);
        }

        await _keyApiService.SaveNewKeys(roomPublicId, publicKeys);
    }

    public async Task SyncAndRotateKey(Guid roomPublicId)
    {
        var roomData = await _roomApiService.GetRoom(roomPublicId);
        
        var currentMemberIds = roomData.Data.ChatRoomKeyBlobDtos
            .Select(x => x.UserPublicId)
            .Distinct()
            .ToList();
        
        var keysData = await _keyApiService.GetPublicIdentities(currentMemberIds);
        
        await RotateRoomKey(roomPublicId, keysData.Data);
    }

    public async Task ShareKeyDataWithUser(Guid roomPublicId, long version, IEnumerable<UserKeyDataDto> userKeys)
    {
        var roomKeyData = await SecureStorage.GetAsync($"{RoomKeyKey}_{roomPublicId}_{version}");
        
        var roomKey = Convert.FromBase64String(roomKeyData);
        
        var encryptedKeys =  new List<RotationDto>();

        foreach (var device in userKeys)
        {
            var newUserKey = Convert.FromBase64String(device.PublicKey);
            var encryptedRoomKey = await _cryptoService.EncryptRoomKey(roomKey, newUserKey);
            
            encryptedKeys.Add(new RotationDto
            {
                UserId =  device.PublicUserId.Value,
                EncryptedRoomKey = encryptedRoomKey,
                Version = version,
                IsHost = false,
                DeviceId = device.DeviceId
            });
        }
        
        await _keyApiService.SaveNewKeys(roomPublicId, encryptedKeys);
    }
}