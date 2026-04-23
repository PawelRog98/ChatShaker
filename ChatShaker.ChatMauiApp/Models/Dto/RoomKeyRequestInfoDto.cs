namespace ChatShaker.ChatMauiApp.Models.Dto;

public class RoomKeyRequestInfoDto
{
    public Guid PublicId { get; set; }
    public long Version { get; set; }
    public string DeviceId { get; set; }
}