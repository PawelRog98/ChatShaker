namespace ChatShaker.Application.Chats.Queries.GetRoom;

public class RoomDto
{
    public Guid? ChatRoomPublicId { get; set; }
    public long HostId { get; set; }
    public string Name { get; set; }
    public DateTime CreateDateUtc { get; set; }
    public List<RoomKeyDto> ChatRoomKeyBlobDtos { get; set; }
}