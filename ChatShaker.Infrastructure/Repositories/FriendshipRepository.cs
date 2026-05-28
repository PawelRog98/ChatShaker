using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;

namespace ChatShaker.Infrastructure.Repositories;

public class FriendshipRepository :  IFriendshipRepository
{
    private readonly AppDbContext _context;
    
    public FriendshipRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Add(Friendship friendship, CancellationToken cancellationToken)
    {
        await _context.Friendships.AddAsync(friendship, cancellationToken);
    }
}