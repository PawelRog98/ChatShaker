using ChatShaker.Domain.Enums;

namespace ChatShaker.Application.Chats.Queries.GetUserRooms;

public class ChatListItemDto
{
    public Guid RoomPublicId { get; set; }
    public string Name { get; set; }
    public string LastMessagePreview { get; set; }
    public string LastMessageNonce { get; set; }
    public MessageTypeEnum Type { get; set; }
    public DateTime LastMessageDate { get; set; }
    public bool IsRead { get; set; }
    public long KeyVersion { get; set; }
}