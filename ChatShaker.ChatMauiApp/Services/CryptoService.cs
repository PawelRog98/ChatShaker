using System.Security.Cryptography;
using System.Text;
using ChatShaker.ChatMauiApp.Models.Dto;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using NSec.Cryptography;

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
        var alg = KeyAgreementAlgorithm.X25519;

        using var ephKey = new Key(KeyAgreementAlgorithm.X25519,
            new KeyCreationParameters
            {
                ExportPolicy = KeyExportPolicies.None
            });

        var recipientKey = PublicKey.Import(alg, recipientPublicKeyBytes, KeyBlobFormat.RawPublicKey);

        using var secret = alg.Agree(ephKey, recipientKey);

        var kek = HkdfDeriveKey(secret, null, "Chatshaker v1", 32);

        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[roomKey.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(kek);
        aes.Encrypt(nonce, roomKey, cipher, tag);

        var ephPublicKey = ephKey.PublicKey.Export(KeyBlobFormat.RawPublicKey);

        var result = new byte[nonce.Length + cipher.Length + tag.Length + ephPublicKey.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(cipher, 0, result, nonce.Length, cipher.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + cipher.Length, tag.Length);
        Buffer.BlockCopy(ephPublicKey, 0, result, nonce.Length + cipher.Length + tag.Length, ephPublicKey.Length);

        return Task.FromResult(Convert.ToBase64String(result));
    }

    public Task<byte[]> DecryptRoomKey(string encryptedDataBase64, byte[] recipientPrivateKeyBytes)
    {
        var combined = Convert.FromBase64String(encryptedDataBase64);

        var nonce = combined[..12];
        var cipher = combined[12..^48];
        var tag = combined[^48..^32];
        var ephPublicKey = combined[^32..];

        var alg = KeyAgreementAlgorithm.X25519;

        var ephSenderKey = PublicKey.Import(alg, ephPublicKey, KeyBlobFormat.RawPublicKey);

        using var recipientKey = Key.Import(alg, recipientPrivateKeyBytes, KeyBlobFormat.RawPrivateKey);

        using var secret = alg.Agree(recipientKey, ephSenderKey);

        var kek = HkdfDeriveKey(secret, null, "Chatshaker v1", 32);

        var roomKey = new byte[cipher.Length];

        using var aes = new AesGcm(kek);
        aes.Decrypt(nonce, cipher, tag, roomKey);

        return Task.FromResult(roomKey);
    }

    public async Task SaveIdentityKey(string userId)
    {
        var existingKey = await SecureStorage.GetAsync(PrivateIdentityKeyKey);
        if (existingKey != null)
            return;

        using var key = new Key(KeyAgreementAlgorithm.X25519,
            new KeyCreationParameters
            {
                ExportPolicy = KeyExportPolicies.AllowPlaintextExport
            });

        var privateKey = key.Export(KeyBlobFormat.RawPrivateKey);
        var publicKey = key.PublicKey.Export(KeyBlobFormat.RawPublicKey);

        await SecureStorage.SetAsync(PrivateIdentityKeyKey, Convert.ToBase64String(privateKey));
        await SecureStorage.SetAsync(PublicIdentityKeyKey, Convert.ToBase64String(publicKey));
        
        var newUserKeyData = new UserKeyDataDto{
            PublicKey = Convert.ToBase64String(publicKey),
            DeviceId = $"{DeviceInfo.Current.VersionString}@{DeviceInfo.Current.Model}"
        };
        await _keyApiService.UploadIdentity(newUserKeyData);
        //UploadPublicKey(userId, publicKey);
    }

    private static byte[] HkdfDeriveKey(SharedSecret secret, byte[]? salt, string info, int length)
    {
        salt ??= Encoding.UTF8.GetBytes("ChatShaker-v1-salt");
        var infoBytes = Encoding.UTF8.GetBytes(info);

        return KeyDerivationAlgorithm.HkdfSha256.DeriveBytes(secret, salt, infoBytes, length);
    }
}
