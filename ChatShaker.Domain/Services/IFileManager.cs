using Microsoft.AspNetCore.Http;

namespace ChatShaker.Domain.Services;

public interface IFileManager
{
    Task<string> UploadEncryptedFile(IFormFile file);
    Task<FileStream?> DownloadFile(string fileName, DateTime createdAtUtc);
}