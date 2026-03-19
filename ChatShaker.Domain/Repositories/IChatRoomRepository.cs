using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IChatRoomRepository
{
    Task<IEnumerable<ChatRoom>> GetUserRooms(long userId, CancellationToken cancellationToken);
    Task<ChatRoom?> GetById(long id, CancellationToken cancellationToken);
    Task<ChatRoom?> GetByPublicId(Guid publicId, CancellationToken cancellationToken);
    Task<IEnumerable<ChatRoom>> GetByUserId(long userId, CancellationToken cancellationToken);
    Task Add(ChatRoom room, CancellationToken cancellationToken);
    Task<long> GetNewestRoomVersion(Guid publicId, CancellationToken cancellationToken);
}
