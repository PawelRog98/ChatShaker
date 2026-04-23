using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly AppDbContext _context;

    public ChatRoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Add(ChatRoom room, CancellationToken cancellationToken)
    {
        await _context.ChatRooms.AddAsync(room, cancellationToken);
    }

    public async Task<IEnumerable<ChatRoom>> GetUserRooms(long userId, CancellationToken cancellationToken)
    {
        return await _context.ChatRoomMemberships
            .Include(x => x.ChatRoom)
            .Where(x => x.UserId == userId)
            .Select(x => x.ChatRoom)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatRoom?> GetById(long id, CancellationToken cancellationToken)
    {
        return await _context.ChatRooms
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ChatRoom?> GetByPublicId(Guid publicId, CancellationToken cancellationToken)
    {
        return await _context.ChatRooms
            .Include(x=>x.ChatRoomKeyBlobs)
            .ThenInclude(x=>x.User)
            .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<IEnumerable<ChatRoom>> GetByUserId(long userId, CancellationToken cancellationToken)
    {
        return await _context.ChatRooms
            .Where(x => x.HostId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetNewestRoomVersion(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _context.ChatRoomKeyBlobs
            .Where(x => x.ChatRoom.PublicId == publicId)
            .Select(x => (long?)x.Version)
            .MaxAsync(cancellationToken);

        return result ?? 0;
    }
}
