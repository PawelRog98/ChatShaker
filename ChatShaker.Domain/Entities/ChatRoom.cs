using ChatShaker.Domain.Abstractions;

namespace ChatShaker.Domain.Entities;

public class ChatRoom : ICommonData
{
    public long Id { get; set; }    
    public Guid PublicId { get; set; }
    public long HostId { get; set; }
    public User Host { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
