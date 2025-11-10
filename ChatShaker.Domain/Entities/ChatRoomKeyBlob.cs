namespace ChatShaker.Domain.Entities;

public class ChatRoomKeyBlob
{
    public long ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string EncryptedRoomKey { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
