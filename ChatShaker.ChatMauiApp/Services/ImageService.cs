using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using ChatShaker.ChatMauiApp.Services.Interfaces;

namespace ChatShaker.ChatMauiApp.Services;

public class ImageService : IImageService
{
    public async Task<byte[]> GenerateThumbnail(string path, int maxWidth = 200, int maxHeight = 200)
    {
        return await Task.Run(async () =>
        {
            using var stream = File.OpenRead(path);
            
            var image = PlatformImage.FromStream(stream);
            
            if (image == null)
                throw new Exception("Could not load image from path.");

            var thumbnail = image.Downsize(maxWidth, true);

            using var memoryStream = new MemoryStream();
            await thumbnail.SaveAsync(memoryStream, ImageFormat.Jpeg);
            
            return memoryStream.ToArray();
        });
    }
}
