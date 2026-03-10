using System.ComponentModel.DataAnnotations.Schema;
using ChatShaker.Domain.Abstractions;
using ChatShaker.Domain.Enums;

namespace ChatShaker.Domain.Entities;

public class ChatRoom : ICommonData
{
    public ChatRoom()
    {
        ChatRoomMemberships = new HashSet<ChatRoomMembership>();
        ChatRoomKeyBlobs = new HashSet<ChatRoomKeyBlob>();
    }
    
    public long Id { get; set; }    
    public Guid PublicId { get; set; }
    public long HostId { get; set; }
    public User Host { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Type { get; set; }
    public bool IsInitialized { get; set; }
    public ICollection<ChatRoomMembership> ChatRoomMemberships { get; set; }
    public ICollection<ChatRoomKeyBlob> ChatRoomKeyBlobs { get; set; }

    [NotMapped]
    public ChatRoomType TypeEnum
    {
        get => Enum.TryParse<ChatRoomType>(Type, out var result) ? result : default;
        set => Type = value.ToString();
    }
}
