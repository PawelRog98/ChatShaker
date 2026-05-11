using ChatShaker.Domain.Enums;

namespace ChatShaker.Application.MessagesManagment.Commands.SendMessage;

public class SendMessageDto
{
    public Guid ChatRoomPublicId { get; set; }
    public Guid? RelatedToPublicId { get; set; }
    public string CipherText { get; set; }
    public string Nonce { get; set; }
    public Guid ClientMessageId { get; set;}
    public MessageTypeEnum MessageType { get; set; }
}
