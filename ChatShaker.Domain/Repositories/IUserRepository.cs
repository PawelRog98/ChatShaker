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
        Task<bool> IsAnyUsers(CancellationToken cancellationToken);
        Task SaveNewUser(User user, CancellationToken cancellationToken);
    }
}
