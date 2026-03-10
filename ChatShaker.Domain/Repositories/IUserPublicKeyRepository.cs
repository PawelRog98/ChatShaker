using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IUserPublicKeyRepository
{
    Task SaveIdentity(UserPublicKey userPublicKey, CancellationToken cancellationToken);
    Task<List<UserPublicKey>> GetUserIdentities(List<Guid> userIdentities, CancellationToken cancellationToken);
}