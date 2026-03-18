using ChatShaker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken);
        Task<User?> GetUserByPublicId(Guid publicId, CancellationToken cancellationToken);
        Task<User?> GetUserById(long id, CancellationToken cancellationToken);
        Task<User?> GetUserByCode(string invitationCode, CancellationToken cancellationToken);
        Task<IEnumerable<User>> GetUsersByPublicId(List<Guid> publicIds, CancellationToken cancellationToken);
        Task<bool> IsAnyUsers(CancellationToken cancellationToken);
        Task SaveNewUser(User user, CancellationToken cancellationToken);
        Task<List<User>> GetFriends(long userId, CancellationToken cancellationToken);
    }
}
