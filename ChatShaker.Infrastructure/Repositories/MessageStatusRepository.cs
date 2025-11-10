using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.Infrastructure.Repositories;

public class MessageStatusRepository : IMessageStatusRepository
{
    private readonly AppDbContext _context;
    public MessageStatusRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MessageStatus?> GetStatus(long messageId, CancellationToken cancellationToken)
    {
        return await _context.MessageStatuses
            .FirstOrDefaultAsync(x => x.MessageId == messageId, cancellationToken);
    }

    public async Task SetStatus(MessageStatus status, CancellationToken cancellationToken)
    {
        await _context.MessageStatuses.AddAsync(status, cancellationToken);
    }
}
