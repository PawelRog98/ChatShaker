namespace ChatShaker.ChatMauiApp.Models.Dto;

public class RoomKeyDataDto
{
    public Guid UserId { get; set; }
    public string EncryptedRoomKey {get; set;}
    public bool? IsHost { get; set; }
}