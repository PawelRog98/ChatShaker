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

    public async Task<IEnumerable<Message>> GetByRoom(long roomId, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .Where(x => x.ChatRoomId == roomId)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveStatus(MessageStatus messageStatus, CancellationToken cancellationToken)
    {
        await _context.MessageStatuses
            .AddAsync(messageStatus, cancellationToken);
    }
}
