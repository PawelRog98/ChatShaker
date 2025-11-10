using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IChatRoomKeyBlobRepository
{
    Task Add(ChatRoomKeyBlob blob, CancellationToken cancellationToken);
    Task<ChatRoomKeyBlob> Get(long roomId, long userId, CancellationToken cancellationToken);
    Task<IEnumerable<ChatRoomKeyBlob>> GetByRoomId(long roomId, CancellationToken cancellationToken);
}
