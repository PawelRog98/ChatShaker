namespace ChatShaker.Application.MessagesManagment.Commands.SendMessage;

public class MessageDto
{
    public Guid PublicId { get; set; }
    public Guid SenderPublicId { get; set; }
    public string CipherText { get; set; }
    public string Nonce { get; set; }
    public DateTime SentAtUtc { get; set; }
    public Guid ClientMessageId { get; set; }
}
