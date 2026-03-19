namespace ChatShaker.Application.Keys.GetRoomKey;

public class RoomKeyReqestDto
{
    public Guid PublicId { get; set; }
    public long Version { get; set; }
    public string DeviceId { get; set; }
}