using ChatShaker.Domain.Enums;

namespace ChatShaker.Domain.Entities;

public class MessageStatus
{
    public long MessageId { get; set; }
    public Message Message { get; set; }
    public MessageStatusEnum Status { get; set; }
    public DateTime UpdateAtUtc { get; set; }

    public void SetAsRead()
        => Status = MessageStatusEnum.Read;

    public void SetAsDelivered()
        => Status = MessageStatusEnum.Delivered;
}