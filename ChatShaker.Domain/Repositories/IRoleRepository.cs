using ChatShaker.Domain.Entities;

namespace ChatShaker.Domain.Repositories;

public interface IRoleRepository
{
    Task<Role> GetIdByName(string name, CancellationToken cancellationToken);
}