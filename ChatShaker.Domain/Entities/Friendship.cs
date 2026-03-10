using ChatShaker.Domain.Abstractions;

namespace ChatShaker.Domain.Entities;

public class Friendship : ICommonData
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long User1Id { get; set; }
    public User User1 { get; set; }
    public long User2Id { get; set; }
    public User User2 { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}