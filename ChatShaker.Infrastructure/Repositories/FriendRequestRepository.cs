using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class FriendRequestRepository : IFriendRequestRepository
{
    private readonly AppDbContext _context;
    public FriendRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveRequest(FriendRequest request, CancellationToken cancellationToken)
    {
        await _context.FriendRequests.AddAsync(request);
    }

    public async Task<FriendRequest> FindByPublicId(Guid publicId, CancellationToken cancellationToken)
    {
        return await _context.FriendRequests
            .FirstOrDefaultAsync(x=>x.PublicId == publicId, cancellationToken);
    }

    public async Task<List<FriendRequest>> GetSentFriendRequests(long userId, CancellationToken cancellationToken)
    {
        return await _context.FriendRequests
            .Include(x=>x.Sender)
            .Where(x=>x.SenderId == userId && x.Status == FriendRequestStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FriendRequest>> GetRecievedFriendRequests(long userId, CancellationToken cancellationToken)
    {
        return await _context.FriendRequests
            .Include(x=>x.Recipient)
            .Where(x=>x.RecipientId == userId && x.Status == FriendRequestStatus.Pending)
            .ToListAsync(cancellationToken);
    }
}