using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class ChatRoomKeyBlobRepository : IChatRoomKeyBlobRepository
{
    private readonly AppDbContext _context;

    public ChatRoomKeyBlobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Add(ChatRoomKeyBlob blob, CancellationToken cancellationToken)
    {
        await _context.ChatRoomKeyBlobs.AddAsync(blob, cancellationToken);
    }

    public async Task<ChatRoomKeyBlob?> Get(Guid roomId, long userId, long version, string deviceId, CancellationToken cancellationToken)
    {
        return await _context.ChatRoomKeyBlobs
            .FirstOrDefaultAsync(x => x.UserId == userId && 
                                      x.ChatRoom.PublicId == roomId &&
                                      x.Version == version &&
                                      x.DeviceId == deviceId, cancellationToken);
    }

    public async Task<IEnumerable<ChatRoomKeyBlob>> GetByRoomId(long roomId, CancellationToken cancellationToken)
    {
        return await _context.ChatRoomKeyBlobs
            .Where(x => x.ChatRoomId == roomId)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveNewRotation(List<ChatRoomKeyBlob> keysData, CancellationToken cancellationToken)
    {
        await _context.AddRangeAsync(keysData, cancellationToken);
    }
}
