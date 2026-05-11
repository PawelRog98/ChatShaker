using System.Security.Cryptography;
namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface ICryptoService
{
    public Task<(string CipherMessageBase64, string NonceBase64)> EncryptMessage(byte[] roomKey, string plainText);
    public Task<string> DecryptMessage(byte[] roomKey, string cipherMessageBase64, string nonceBase64);
    public Task<(byte[] CipherBytes, byte[] Nonce)> EncryptBytes(byte[] roomKey, byte[] plainBytes);
    public Task<byte[]> DecryptBytes(byte[] roomKey, byte[] cipherBytes, byte[] nonce);
    Task<string> EncryptRoomKey(byte[] roomKey, byte[] recipientPublicKeyBytes);
    Task<byte[]> DecryptRoomKey(string encryptedDataBase64, byte[] recipientPrivateKeyBytes);
    Task SaveIdentityKey(string userId);
}
