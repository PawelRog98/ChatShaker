using ChatShaker.ChatMauiApp.Models.Enums;
using ChatShaker.ChatMauiApp.Models.Local;

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
    public MessageTypeEnum MessageType { get; set; }

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

    private ImageMessageContent _imageContent;
    public ImageMessageContent ImageContent
    {
        get => _imageContent;
        set => SetProperty(ref _imageContent, value);
    }

    private string _thumbnailLocalPath;
    public string ThumbnailLocalPath
    {
        get => _thumbnailLocalPath;
        set => SetProperty(ref _thumbnailLocalPath, value);
    }

    private string _fullImageLocalPath;
    public string FullImageLocalPath
    {
        get => _fullImageLocalPath;
        set => SetProperty(ref _fullImageLocalPath, value);
    }

    private bool _isDownloadingFullImage;
    public bool IsDownloadingFullImage
    {
        get => _isDownloadingFullImage;
        set => SetProperty(ref _isDownloadingFullImage, value);
    }

    public string DisplayText => MessageType == MessageTypeEnum.Text ? CipherText : ImageContent?.Text;
}
