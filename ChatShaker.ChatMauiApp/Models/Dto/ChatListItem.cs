namespace ChatShaker.ChatMauiApp.Models.Dto;

public class ChatListItem
{
    public Guid RoomPublicId { get; set; }
    public string Name { get; set; }
    public string LastMessagePreview { get; set; }
    public DateTime LastMessageDate { get; set; }
    bool IsRead { get; set; }
}
