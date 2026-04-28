using ChatShaker.Domain.Entities;

namespace ChatShaker.Application.Files.Queries;

public class FileResultModel
{
    public FileStream FileStream { get; set; }
    public string FileName { get; set; }
    public string? ContentType { get; set; }
}