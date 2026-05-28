namespace ChatShaker.ChatMauiApp.Models.Dto;

public class RoomKeyBlobDto
{
    public Guid UserPublicId { get; set; }
    public string EncryptedRoomKey { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string DeviceId  { get; set; }
}

public class ChatRoomDto
{
    public Guid ChatRoomPublicId { get; set; }
    public List<RoomKeyBlobDto> ChatRoomKeyBlobDtos { get; set; }
}
