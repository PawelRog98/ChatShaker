using System.Security.Cryptography;
using System.Text;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace ChatShaker.ChatMauiApp.Services;

public class CryptoService : ICryptoService
{
    private const string PrivateIdentityKeyKey = "identity_private_key_";
    private const string PublicIdentityKeyKey = "identity_public_key_";
    private const string SaltValue = "ChatShaker-v1-salt";
    private const string InfoValue = "Chatshaker v1";

    private readonly IKeyApiService _keyApiService;

    public CryptoService(IKeyApiService keyApiService)
    {
        _keyApiService = keyApiService;
    }

    public async Task<(string CipherMessageBase64, string NonceBase64)> EncryptMessage(byte[] roomKey, string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var (cipherBytes, nonce) = await EncryptBytes(roomKey, plainTextBytes);
        
        return (Convert.ToBase64String(cipherBytes), Convert.ToBase64String(nonce));
    }

    public async Task<string> DecryptMessage(byte[] roomKey, string cipherMessageBase64, string nonceBase64)
    {
        var cipherBytes = Convert.FromBase64String(cipherMessageBase64);
        var nonce = Convert.FromBase64String(nonceBase64);
        
        var plainBytes = await DecryptBytes(roomKey, cipherBytes, nonce);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public Task<(byte[] CipherBytes, byte[] Nonce)> EncryptBytes(byte[] roomKey, byte[] plainBytes)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);

        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(roomKey), 128, nonce);
        cipher.Init(true, parameters);

        var cipherTextBytes = new byte[cipher.GetOutputSize(plainBytes.Length)];
        var len = cipher.ProcessBytes(plainBytes, 0, plainBytes.Length, cipherTextBytes, 0);
        len += cipher.DoFinal(cipherTextBytes, len);

        return Task.FromResult((cipherTextBytes, nonce));
    }

    public Task<byte[]> DecryptBytes(byte[] roomKey, byte[] cipherBytes, byte[] nonce)
    {
        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(roomKey), 128, nonce);
        cipher.Init(false, parameters);
        
        var plainBytes = new byte[cipher.GetOutputSize(cipherBytes.Length)];
        var len = cipher.ProcessBytes(cipherBytes, 0, cipherBytes.Length, plainBytes, 0);
        len += cipher.DoFinal(plainBytes, len);

        if (len < plainBytes.Length)
        {
            var trimmedBytes = new byte[len];
            Buffer.BlockCopy(plainBytes, 0, trimmedBytes, 0, len);
            return Task.FromResult(trimmedBytes);
        }
        
        return Task.FromResult(plainBytes);
    }

    public Task<string> EncryptRoomKey(byte[] roomKey, byte[] recipientPublicKeyBytes)
    {
        var generator = new X25519KeyPairGenerator();
        generator.Init(new X25519KeyGenerationParameters(new SecureRandom()));
        var ephKeyPair = generator.GenerateKeyPair();

        var ephPrivateKey = (X25519PrivateKeyParameters)ephKeyPair.Private;
        var ephPublicKey = (X25519PublicKeyParameters)ephKeyPair.Public;

        var recipientPublicKey = new X25519PublicKeyParameters(recipientPublicKeyBytes, 0);

        byte[] secret = new byte[32];
        ephPrivateKey.GenerateSecret(recipientPublicKey, secret, 0);

        var kek = HkdfDeriveKey(secret, Encoding.UTF8.GetBytes(SaltValue), InfoValue, 32);

        var nonce = RandomNumberGenerator.GetBytes(12);
        
        var gcmCipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(kek), 128, nonce);
        gcmCipher.Init(true, parameters);

        var cipherTextBytes = new byte[gcmCipher.GetOutputSize(roomKey.Length)];
        var len = gcmCipher.ProcessBytes(roomKey, 0, roomKey.Length, cipherTextBytes, 0);
        len += gcmCipher.DoFinal(cipherTextBytes, len);

        var ephPublicKeyBytes = ephPublicKey.GetEncoded();

        var result = new byte[nonce.Length + len + ephPublicKeyBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(cipherTextBytes, 0, result, nonce.Length, len);
        Buffer.BlockCopy(ephPublicKeyBytes, 0, result, nonce.Length + len, ephPublicKeyBytes.Length);

        return Task.FromResult(Convert.ToBase64String(result));
    }

    public Task<byte[]> DecryptRoomKey(string encryptedDataBase64, byte[] recipientPrivateKeyBytes)
    {
        if (string.IsNullOrEmpty(encryptedDataBase64))
            throw new ArgumentNullException(nameof(encryptedDataBase64));

        var combined = Convert.FromBase64String(encryptedDataBase64);

        if (combined.Length < 60)
            throw new ArgumentException("Encrypted data is too short.", nameof(encryptedDataBase64));

        var nonce = combined[..12];
        var encryptedKeyWithTag = combined[12..^32];
        var ephPublicKeyBytes = combined[^32..];

        var recipientPrivateKey = new X25519PrivateKeyParameters(recipientPrivateKeyBytes, 0);
        var ephSenderPublicKey = new X25519PublicKeyParameters(ephPublicKeyBytes, 0);

        byte[] secret = new byte[32];
        recipientPrivateKey.GenerateSecret(ephSenderPublicKey, secret, 0);

        var kek = HkdfDeriveKey(secret, Encoding.UTF8.GetBytes(SaltValue), InfoValue, 32);

        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(kek), 128, nonce);
        cipher.Init(false, parameters);

        var roomKey = new byte[cipher.GetOutputSize(encryptedKeyWithTag.Length)];
        var len = cipher.ProcessBytes(encryptedKeyWithTag, 0, encryptedKeyWithTag.Length, roomKey, 0);
        len += cipher.DoFinal(roomKey, len);

        if (len < roomKey.Length)
        {
            var trimmedRoomKey = new byte[len];
            Buffer.BlockCopy(roomKey, 0, trimmedRoomKey, 0, len);
            return Task.FromResult(trimmedRoomKey);
        }

        return Task.FromResult(roomKey);
    }

    public async Task SaveIdentityKey(string userId)
    {
        var existingKey = await SecureStorage.GetAsync(PrivateIdentityKeyKey + userId);
        if (existingKey != null)
            return;

        var generator = new X25519KeyPairGenerator();
        generator.Init(new X25519KeyGenerationParameters(new SecureRandom()));
        var keyPair = generator.GenerateKeyPair();

        var privateKey = ((X25519PrivateKeyParameters)keyPair.Private).GetEncoded();
        var publicKey = ((X25519PublicKeyParameters)keyPair.Public).GetEncoded();

        await SecureStorage.SetAsync(PrivateIdentityKeyKey + userId, Convert.ToBase64String(privateKey));
        await SecureStorage.SetAsync(PublicIdentityKeyKey + userId, Convert.ToBase64String(publicKey));
        
        var newUserKeyData = new UserKeyDataDto{
            PublicKey = Convert.ToBase64String(publicKey),
            DeviceId = $"{DeviceInfo.Current.VersionString}@{DeviceInfo.Current.Model}"
        };
        await _keyApiService.UploadIdentity(newUserKeyData);
    }

    private static byte[] HkdfDeriveKey(byte[] secret, byte[] salt, string info, int length)
    {
        var infoBytes = Encoding.UTF8.GetBytes(info);
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, secret, length, salt, infoBytes);
    }
}
