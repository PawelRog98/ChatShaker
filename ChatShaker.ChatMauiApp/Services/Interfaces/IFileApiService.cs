using ChatShaker.ChatMauiApp.Helpers;

namespace ChatShaker.ChatMauiApp.Services.Interfaces;

public interface IFileApiService
{
    Task<Response<string>> UploadFile(byte[] fileBytes, string fileName, string contentType, Guid roomId);
    Task<Stream> DownloadFile(Guid publicId);
}
