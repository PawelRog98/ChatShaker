namespace ChatShaker.Domain.Entities;

public class ChatRoomMembership
{
    public long ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public long AddedById { get; set; }
    public User AddedBy { get; set; }
}
