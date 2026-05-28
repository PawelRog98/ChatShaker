using ChatShaker.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ChatShaker.Infrastructure.FileManagement;

public class FileManager : IFileManager
{
    private readonly string _storagePath;
    
    public FileManager(IConfiguration configuration)
    {
        _storagePath = configuration["FileStorage:RootPath"] ?? "Storage/Files";
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
    }
    
    public async Task<string> UploadEncryptedFile(IFormFile file)
    {
        var dateTimeNow = DateTime.UtcNow;
        
        var folderPath = Path.Combine(
            _storagePath, 
            dateTimeNow.Year.ToString(), 
            dateTimeNow.Month.ToString("D2"),
            dateTimeNow.Day.ToString("D2"));
        
        if(!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        
        var fullPath = Path.Combine(folderPath, file.FileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await  file.CopyToAsync(stream);
        
        return fullPath;
    }

    public async Task<FileStream?> DownloadFile(string fileName, DateTime createdAtUtc)
    {
        var folderName = Path.Combine(
            _storagePath,
            createdAtUtc.Year.ToString(),
            createdAtUtc.Month.ToString("D2"),
            createdAtUtc.Day.ToString("D2"));

        var fullPath = Path.Combine(folderName, fileName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File does not exists");
        
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
    }
}