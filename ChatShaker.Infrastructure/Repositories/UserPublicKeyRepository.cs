using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class UserPublicKeyRepository : IUserPublicKeyRepository
{
    private readonly AppDbContext _context;

    public UserPublicKeyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Add(UserPublicKey userPublicKey, CancellationToken cancellationToken)
    {
        await _context.UserPublicKeys.AddAsync(userPublicKey);
    }

    public async Task<List<UserPublicKey>> GetUserIdentities(List<Guid> userIdentities, CancellationToken cancellationToken)
    {
        return await _context.UserPublicKeys
            .Include(x => x.User)
            .Where(x => userIdentities.Contains(x.User.PublicId))
            .ToListAsync(cancellationToken);
    }
}