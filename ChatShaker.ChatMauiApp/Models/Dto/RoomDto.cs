namespace ChatShaker.ChatMauiApp.Models.Dto;

public class RoomDto
{
    public Guid? ChatRoomPublicId { get; set; }
    public long HostId { get; set; }
    public string Name { get; set; }
    public DateTime CreateDateUtc { get; set; }
    public List<RoomKeyDataDto> ChatRoomKeyBlobDtos { get; set; }
}