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

    public async Task<MessageStatus?> GetStatus(long messageId, long userId, CancellationToken cancellationToken)
    {
        return await _context.MessageStatuses
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.UserId == userId, cancellationToken);
    }

    public async Task SetStatus(MessageStatus status, CancellationToken cancellationToken)
    {
        await _context.MessageStatuses.AddAsync(status, cancellationToken);
    }

    public async Task UpdateStatus(MessageStatus status, CancellationToken cancellationToken)
    {
        _context.MessageStatuses.Update(status);
        await Task.CompletedTask;
    }
}
