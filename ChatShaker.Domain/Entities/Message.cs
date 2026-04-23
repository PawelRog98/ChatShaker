using ChatShaker.Domain.Abstractions;

namespace ChatShaker.Domain.Entities;

public class Message : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; }
    public long SenderId { get; set; } 
    public User Sender { get; set; }
    public long? RelatedToId { get; set; }
    public Message? RelatedTo { get; set; }
    public string CipherText { get; set; }
    public string Nonce { get; set; }
    public DateTime SentAtUtc { get; set; }
    public Guid ClientMessageId { get; set; }
    public virtual ICollection<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();

}
