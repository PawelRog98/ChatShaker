using Microsoft.AspNetCore.Http;

namespace ChatShaker.Application.Files.Commands;

public class UploadedFileDto
{
    public IFormFile File { get; set; }
    public Guid RoomId { get; set; }
}