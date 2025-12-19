using System.Security.Cryptography;
namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface ICryptoService
{
    public Task<(string CipherMessageBase64, string NonceBase64)> EncryptMessage(byte[] roomKey, string plainText);
    public Task<string> DecryptMessage(byte[] roomKey, string cipherMessageBase64, string nonceBase64);
    Task<string> EncryptRoomKey(byte[] roomKey, byte[] senderPrivateKeyBytes, byte[] recipientPublicKeyBytes);
    Task<byte[]> DecryptRoomKey(string encryptedRoomKeyBase64, byte[] senderPublicKeyBytes, byte[] recipientPrivateKeyBytes);
}
