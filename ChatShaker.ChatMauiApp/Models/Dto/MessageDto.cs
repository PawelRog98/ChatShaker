using ChatShaker.ChatMauiApp.Models.Enums;

namespace ChatShaker.ChatMauiApp.Models.Dto;

public class MessageDto : BindableBase
{
    public Guid PublicId { get; set; }
    public Guid ChatRoomPublicId { get; set; }
    public Guid? RelatedToPublicId { get; set; }
    public string CipherText { get; set; }
    public Guid? SenderPublicId { get; set; }
    public string SenderName { get; set; }
    public DateTime SentAtUtc {get; set;}
    public string Nonce { get; set; }
    public Guid ClientMessageId { get; set; }

    private MessageStatusEnum _status;
    public MessageStatusEnum Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    private bool _isMine;
    public bool IsMine
    {
        get => _isMine;
        set => SetProperty(ref _isMine, value);
    }
}
