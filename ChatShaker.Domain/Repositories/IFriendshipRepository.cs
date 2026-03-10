using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IFriendshipRepository
{
    Task SaveFriendship(Friendship friendship, CancellationToken cancellationToken);
}