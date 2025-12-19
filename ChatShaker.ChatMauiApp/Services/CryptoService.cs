using System.Security.Cryptography;
using System.Text;
using ChatShaker.ChatMauiApp.Services.Interfaces;
using NSec.Cryptography;

namespace ChatShaker.ChatMauiApp.Services;

public class CryptoService : ICryptoService
{
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

    public Task<string> EncryptRoomKey(byte[] roomKey, byte[] senderPrivateKeyBytes, byte[] recipientPublicKeyBytes)
    {
        var alg = KeyAgreementAlgorithm.X25519;

        using var senderKey = Key.Import(alg, senderPrivateKeyBytes, KeyBlobFormat.RawPrivateKey);
        var recipientKey = PublicKey.Import(alg, recipientPublicKeyBytes, KeyBlobFormat.RawPublicKey);

        using var secret = alg.Agree(senderKey, recipientKey);
        var secretBytes = secret.Export(SharedSecretBlobFormat.RawSharedSecret);

        var kek = HKDF(secretBytes);

        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[roomKey.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(kek);
        aes.Encrypt(nonce, roomKey, cipher, tag);

        var result = new byte[nonce.Length + cipher.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(cipher, 0, result, nonce.Length, cipher.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length + cipher.Length, tag.Length);

        return Task.FromResult(Convert.ToBase64String(result));
    }

    public Task<byte[]> DecryptRoomKey(string encryptedRoomKeyBase64, byte[] senderPublicKeyBytes, byte[] recipientPrivateKeyBytes)
    {
        var combined = Convert.FromBase64String(encryptedRoomKeyBase64);

        var nonce = combined[..12];
        var cipher = combined[12..^16];
        var tag = combined[^16..];

        var alg = KeyAgreementAlgorithm.X25519;

        using var recipientKey = Key.Import(alg, recipientPrivateKeyBytes, KeyBlobFormat.RawPrivateKey);
        var senderKey = PublicKey.Import(alg, senderPublicKeyBytes, KeyBlobFormat.RawPublicKey);

        using var secret = alg.Agree(recipientKey, senderKey);
        var secretBytes = secret.Export(SharedSecretBlobFormat.RawSharedSecret);

        var kek = HKDF(secretBytes);

        var roomKey = new byte[cipher.Length];

        using var aes = new AesGcm(kek);
        aes.Decrypt(nonce, cipher, tag, roomKey);

        return Task.FromResult(roomKey);
    }

    private static byte[] HKDF(byte[] input)
    {
        using var hmac = new HMACSHA256(input);
        return hmac.ComputeHash(Array.Empty<byte>());
    }
}
