namespace ChatShaker.ChatMauiApp.Models.Dto;

public class RoomKeyDataDto
{
    public Guid UserPublicId { get; set; }
    public string EncryptedRoomKey {get; set;}
    public long Version { get; set; }
    public bool? IsHost { get; set; }
    public string DeviceId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}