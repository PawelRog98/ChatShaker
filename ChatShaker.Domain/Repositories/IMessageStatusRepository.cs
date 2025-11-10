using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IMessageStatusRepository
{
    Task SetStatus(MessageStatus status, CancellationToken cancellationToken);
    Task<MessageStatus?> GetStatus(long messageId, CancellationToken cancellationToken);
}
