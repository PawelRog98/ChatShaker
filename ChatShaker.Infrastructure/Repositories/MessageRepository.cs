using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message> Add(Message message, CancellationToken cancellationToken)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
        return message;
    }

    public async Task<IEnumerable<Message>> GetByRoom(long roomId, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .Include(x => x.MessageStatuses)
            .Where(x => x.ChatRoomId == roomId)
            .OrderByDescending(x => x.SentAtUtc)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveStatus(MessageStatus messageStatus, CancellationToken cancellationToken)
    {
        await _context.MessageStatuses
            .AddAsync(messageStatus, cancellationToken);
    }

    public async Task<IEnumerable<Message>> GetLastMessageByRoom(long[] roomIds, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .Include(x=>x.MessageStatuses)
            .Where(x => roomIds.Contains(x.ChatRoomId))
            .GroupBy(x => x.ChatRoomId)
            .Select(g=>g.OrderByDescending(y=>y.SentAtUtc).First())
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Message>> GetByPublicId(Guid publicId, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .Where(x => x.PublicId == publicId)
            .ToListAsync(cancellationToken);
    }
}
