using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IUserPublicKeyRepository
{
    Task Add(UserPublicKey userPublicKey, CancellationToken cancellationToken);
    Task<List<UserPublicKey>> GetUserIdentities(List<Guid> userIdentities, CancellationToken cancellationToken);
}