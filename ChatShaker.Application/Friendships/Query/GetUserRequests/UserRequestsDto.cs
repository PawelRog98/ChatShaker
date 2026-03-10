using ChatShaker.Domain.Enums;

namespace ChatShaker.Application.Friendships.Query;

public class UserRequestsDto
{
    public Guid PublicId { get; set; }
    public string Username { get; set; }
    public DateTime SentAtUtc { get; set; }
    public FriendRequestStatus Status { get; set; }
}