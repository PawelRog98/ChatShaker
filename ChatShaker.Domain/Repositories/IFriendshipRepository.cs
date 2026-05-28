using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IFriendshipRepository
{
    Task Add(Friendship friendship, CancellationToken cancellationToken);
}