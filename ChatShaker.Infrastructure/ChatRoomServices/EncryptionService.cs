using System.Security.Cryptography;
using ChatShaker.Domain.Services;

namespace ChatShaker.Infrastructure.ChatRoomServices;

public class EncryptionService : IEncryptionService
{
    public byte[] GenerateRoomKey()
        => RandomNumberGenerator.GetBytes(32);
}
