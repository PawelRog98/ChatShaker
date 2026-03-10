using ChatShaker.Domain.Abstractions;

namespace ChatShaker.Domain.Entities;

public class UserPublicKey : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string PublicKey { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string DeviceId {get;set;}
}
