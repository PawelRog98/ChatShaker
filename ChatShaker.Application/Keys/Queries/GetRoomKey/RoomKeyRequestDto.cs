namespace ChatShaker.Application.Keys.GetRoomKey;

public class RoomKeyRequestDto
{
    public Guid PublicId { get; set; }
    public long Version { get; set; }
    public string DeviceId { get; set; }
}