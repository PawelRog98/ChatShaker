namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IImageService
{
    Task<byte[]> GenerateThumbnail(string path, int maxWidth = 200, int maxHeight = 200);
}
