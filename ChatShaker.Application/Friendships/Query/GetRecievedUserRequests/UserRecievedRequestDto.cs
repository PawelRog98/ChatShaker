using ChatShaker.Domain.Enums;

namespace ChatShaker.Application.Friendships.Query.GetRecievedUserRequests;

public class UserRecievedRequestDto
{
    public Guid PublicId { get; set; }
    public string UsernameSender { get; set; }
    public DateTime SentAtUtc { get; set; }
    public FriendRequestStatus Status { get; set; }
}