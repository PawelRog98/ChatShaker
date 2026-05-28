namespace ChatShaker.ChatMauiApp.Models.Dto;

public class RotationDto
{
    public Guid UserId { get; set; }
    public string EncryptedRoomKey {get; set;}
    public long Version { get; set; }
    public bool? IsHost { get; set; }
    public string DeviceId { get; set; }
}
