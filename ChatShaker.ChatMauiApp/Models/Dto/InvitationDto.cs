namespace ChatShaker.ChatMauiApp.Models.Dto;

public class InvitationDto
{
    public Guid PublicId { get; set; }
    public string UserName { get; set; }
    public string Status {get; set;}
    public DateTime SentAtUtc { get; set; }
}