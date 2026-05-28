namespace ChatShaker.ChatMauiApp.Models.Dto;

public class CreateChatRoomDto
{
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<UserEncryptionDto> Keys { get; set; }
}

public class UserEncryptionDto
{
    public Guid UserId { get; set; }
    public string EncryptedUserKey { get; set; }
    public bool IsHost { get; set; }
    public string DeviceId { get; set; }
    public long Version { get; set; }
}
