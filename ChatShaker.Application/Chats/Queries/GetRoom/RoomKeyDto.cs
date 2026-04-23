namespace ChatShaker.Application.Chats.Queries.GetRoom;

public class RoomKeyDto
{
    public Guid UserPublicId { get; set; }
    public string EncryptedRoomKey {get; set;}
    public long Version { get; set; }
    public bool? IsHost { get; set; }
    public string DeviceId { get; set; }
}