namespace ChatShaker.Domain.Services;

public interface IEncryptionService
{
    byte[] GenerateRoomKey();
}
