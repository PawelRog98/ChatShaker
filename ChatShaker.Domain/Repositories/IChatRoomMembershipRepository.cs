using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IChatRoomMembershipRepository
{
    Task<IEnumerable<ChatRoomMembership>> GetByRoomId(long roomId, CancellationToken cancellationToken);
    Task Add(ChatRoomMembership member, CancellationToken cancellationToken);
    Task Remove(ChatRoomMembership chatRoomMembership, CancellationToken cancellationToken);
}
