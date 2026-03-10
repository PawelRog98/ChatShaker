using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetByRoom(long roomId, int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task<Message> Add(Message message, CancellationToken cancellationToken);
    Task SaveStatus(MessageStatus messageStatus, CancellationToken cancellationToken);
    Task<IEnumerable<Message>> GetLastMessageByRoom(long[] roomIds, CancellationToken cancellationToken);
}
