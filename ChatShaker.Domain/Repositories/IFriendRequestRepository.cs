using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IFriendRequestRepository
{
    Task SaveRequest(FriendRequest request, CancellationToken cancellationToken);
    Task<FriendRequest> FindByPublicId(Guid publicId, CancellationToken cancellationToken);
    Task<List<FriendRequest>> GetSentFriendRequests(long userId, CancellationToken cancellationToken);
    Task<List<FriendRequest>> GetRecievedFriendRequests(long userId, CancellationToken cancellationToken);
}