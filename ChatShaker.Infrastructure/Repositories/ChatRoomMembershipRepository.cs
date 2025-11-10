using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class ChatRoomMembershipRepository : IChatRoomMembershipRepository
{
    private readonly AppDbContext _context;

    public ChatRoomMembershipRepository(AppDbContext context)
    {
        _context = context;
    }
 
    public async Task Add(ChatRoomMembership member, CancellationToken cancellationToken)
    {
        await _context.ChatRoomMemberships.AddAsync(member, cancellationToken);
    }

    public async Task<IEnumerable<ChatRoomMembership>> GetByRoomId(long roomId, CancellationToken cancellationToken)
    {
        return await _context.ChatRoomMemberships
            .Where(x => x.ChatRoomId == roomId)
            .ToListAsync(cancellationToken);
    }

    public async Task Remove(ChatRoomMembership chatRoomMembership, CancellationToken cancellationToken)
    {
        _context.ChatRoomMemberships.Remove(chatRoomMembership);
    }
}
