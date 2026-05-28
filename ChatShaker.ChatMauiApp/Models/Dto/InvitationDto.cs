using ChatShaker.ChatMauiApp.Models.Enums;

namespace ChatShaker.ChatMauiApp.Models.Dto;

public class InvitationDto
{
    public Guid PublicId { get; set; }
    public string SenderUsername { get; set; }
    public string RecipientUsername { get; set; }
    public FriendRequestStatus Status {get; set;}
    public DateTime SentAtUtc { get; set; }
}