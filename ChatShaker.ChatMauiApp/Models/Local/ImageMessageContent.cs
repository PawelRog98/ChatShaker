namespace ChatShaker.ChatMauiApp.Models.Local;

public class ImageMessageContent
{
    public string FilePublicId { get; set; }
    public string ThumbnailPublicId {get; set;}
    public string Text { get; set; }
    public long KeyVersion { get; set; }
}