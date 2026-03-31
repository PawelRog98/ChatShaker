namespace ChatShaker.ChatMauiApp.Models.Dto;

public class AcceptanceDecisionDto
{
    public Guid InvitationRequestId { get; set; }
    public bool IsAccepted { get; set; }
}