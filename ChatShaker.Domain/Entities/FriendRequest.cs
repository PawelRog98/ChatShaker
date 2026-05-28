using ChatShaker.Domain.Abstractions;
using ChatShaker.Domain.Enums;

namespace ChatShaker.Domain.Entities;

public class FriendRequest : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long SenderId { get; set; }
    public User Sender { get; set; }
    public long RecipientId { get; set; }
    public User Recipient { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public FriendRequestStatus Status { get; set; }
}