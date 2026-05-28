namespace ChatShaker.Application.Friendships.Commands.AcceptInvitation;

public class AcceptanceDecisionDto
{
    public Guid InvitationRequestId { get; set; }
    public bool IsAccepted { get; set; }
}