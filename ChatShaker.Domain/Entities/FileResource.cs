using System.ComponentModel.DataAnnotations.Schema;
using ChatShaker.Domain.Abstractions;
using ChatShaker.Domain.Enums;

namespace ChatShaker.Domain.Entities;

public class FileResource : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string FileName { get; set; }
    public string? Description { get; set; }
    public string? ContentType { get; set; }
    public long? ChatRoomId { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Type  { get; set; }
    
    [NotMapped]
    public FileType TypeEnum 
    {
        get => Enum.TryParse(Type, true, out FileType fileType) ? fileType : FileType.Document; 
        set => Type = value.ToString(); 
    }
}