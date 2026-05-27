using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IChatRoomKeyBlobRepository
{
    Task Add(ChatRoomKeyBlob blob, CancellationToken cancellationToken);
    Task<ChatRoomKeyBlob> Get(Guid roomId, long userId, long version, string deviceId, CancellationToken cancellationToken);
    Task<IEnumerable<ChatRoomKeyBlob>> GetByRoomId(long roomId, CancellationToken cancellationToken);
    Task AddRange(List<ChatRoomKeyBlob> keysData, CancellationToken cancellationToken);
    Task<bool> GetInfoIsUserHasActiveKey(long roomId, long userId, long newestKeyVersion, CancellationToken cancellationToken);
}
