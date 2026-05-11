using ChatShaker.ChatMauiApp.Models.Enums;

namespace ChatShaker.ChatMauiApp.Models.Dto;

public class ChatListItem
{
    public Guid RoomPublicId { get; set; }
    public string Name { get; set; }
    public string LastMessagePreview { get; set; }
    public string LastMessageNonce { get; set; }
    public MessageTypeEnum Type { get; set; }
    public DateTime LastMessageDate { get; set; }
    public bool IsRead { get; set; }
}
