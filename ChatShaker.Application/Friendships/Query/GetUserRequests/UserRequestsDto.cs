using ChatShaker.Domain.Enums;

namespace ChatShaker.Application.Friendships.Query;

public class UserRequestsDto
{
    public Guid PublicId { get; set; }
    public string SenderUsername { get; set; }
    public string RecipientUsername { get; set; }
    public DateTime SentAtUtc { get; set; }
    public FriendRequestStatus Status { get; set; }
}