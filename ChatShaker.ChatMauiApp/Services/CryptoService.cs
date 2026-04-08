using System.Security.Cryptography;
using System.Text;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace ChatShaker.ChatMauiApp.Services;

public class CryptoService : ICryptoService
{
    private const string PrivateIdentityKeyKey = "identity_private_key";
    private const string PublicIdentityKeyKey = "identity_public_key";

    private readonly IKeyApiService _keyApiService;

    public CryptoService(IKeyApiService keyApiService)
    {
        _keyApiService = keyApiService;
    }

    public Task<(string CipherMessageBase64, string NonceBase64)> EncryptMessage(byte[] roomKey, string plainText)
    {
        var nonceBytes = RandomNumberGenerator.GetBytes(12);

        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainTextBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(roomKey);
        aes.Encrypt(nonceBytes, plainTextBytes, cipherBytes, tag);

        var combined = new byte[cipherBytes.Length + tag.Length];
        Buffer.BlockCopy(cipherBytes, 0, combined, 0, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, combined, cipherBytes.Length, tag.Length);

        var cipherText = Convert.ToBase64String(combined);
        var nonce = Convert.ToBase64String(nonceBytes);

        return Task.FromResult((cipherText, nonce));
    }

    public Task<string> DecryptMessage(byte[] roomKey, string cipherMessageBase64, string nonceBase64)
    {
        var combined = Convert.FromBase64String(cipherMessageBase64);
        var nonce = Convert.FromBase64String(nonceBase64);

        var cipher = combined[..^16];
        var tag = combined[^16..];

        var plainText = new byte[cipher.Length];

        using var aes = new AesGcm(roomKey);
        aes.Decrypt(nonce, cipher, tag, plainText);

        var textToReturn = Encoding.UTF8.GetString(plainText);

        return Task.FromResult(textToReturn);
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
        
        var kek = HkdfDeriveKey(secret, null, "Chatshaker v1", 32);

        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[roomKey.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(kek);
        aes.Encrypt(nonce, roomKey, cipher, tag);

        var ephPublicKeyBytes = ephPublicKey.GetEncoded();

        var result = new byte[nonce.Length + cipher.Length + tag.Length + ephPublicKeyBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(cipher, 0, result, nonce.Length, cipher.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + cipher.Length, tag.Length);
        Buffer.BlockCopy(ephPublicKeyBytes, 0, result, nonce.Length + cipher.Length + tag.Length, ephPublicKeyBytes.Length);

        return Task.FromResult(Convert.ToBase64String(result));
    }

    public Task<byte[]> DecryptRoomKey(string encryptedDataBase64, byte[] recipientPrivateKeyBytes)
    {
        var combined = Convert.FromBase64String(encryptedDataBase64);

        var nonce = combined[..12];
        var cipher = combined[12..^48];
        var tag = combined[^48..^32];
        var ephPublicKeyBytes = combined[^32..];

        var recipientPrivateKey = new X25519PrivateKeyParameters(recipientPrivateKeyBytes, 0);
        var ephSenderPublicKey = new X25519PublicKeyParameters(ephPublicKeyBytes, 0);

        byte[] secret = new byte[32];
        recipientPrivateKey.GenerateSecret(ephSenderPublicKey, secret, 0);

        var kek = HkdfDeriveKey(secret, null, "Chatshaker v1", 32);

        var roomKey = new byte[cipher.Length];

        using var aes = new AesGcm(kek);
        aes.Decrypt(nonce, cipher, tag, roomKey);

        return Task.FromResult(roomKey);
    }

    public async Task SaveIdentityKey(string userId)
    {
        SecureStorage.Remove(PrivateIdentityKeyKey);
        
        var existingKey = await SecureStorage.GetAsync(PrivateIdentityKeyKey);
        if (existingKey != null)
            return;

        var generator = new X25519KeyPairGenerator();
        generator.Init(new X25519KeyGenerationParameters(new SecureRandom()));
        var keyPair = generator.GenerateKeyPair();

        var privateKey = ((X25519PrivateKeyParameters)keyPair.Private).GetEncoded();
        var publicKey = ((X25519PublicKeyParameters)keyPair.Public).GetEncoded();

        await SecureStorage.SetAsync(PrivateIdentityKeyKey, Convert.ToBase64String(privateKey));
        await SecureStorage.SetAsync(PublicIdentityKeyKey, Convert.ToBase64String(publicKey));
        
        var newUserKeyData = new UserKeyDataDto{
            PublicKey = Convert.ToBase64String(publicKey),
            DeviceId = $"{DeviceInfo.Current.VersionString}@{DeviceInfo.Current.Model}"
        };
        await _keyApiService.UploadIdentity(newUserKeyData);
    }

    private static byte[] HkdfDeriveKey(byte[] secret, byte[]? salt, string info, int length)
    {
        salt ??= Encoding.UTF8.GetBytes("ChatShaker-v1-salt");
        var infoBytes = Encoding.UTF8.GetBytes(info);

        return HKDF.DeriveKey(HashAlgorithmName.SHA256, secret, length, salt, infoBytes);
    }
}
