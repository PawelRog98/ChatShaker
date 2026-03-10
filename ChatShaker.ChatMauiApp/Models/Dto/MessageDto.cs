using ChatShaker.ChatMauiApp.Models.Enums;

namespace ChatShaker.ChatMauiApp.Models.Dto;

public class MessageDto
{
    public Guid ChatRoomPublicId { get; set; }
    public Guid? RelatedToPublicId { get; set; }
    public string CipherText { get; set; }
    public Guid? SenderPublicId { get; set; }
    public string SenderName { get; set; }
    public DateTime SentDataTimeUtc {get; set;}
    public string Nonce { get; set; }
    public Guid ClientMessageId { get; set; }
    public MessageStatusEnum Status {get; set;}
}
