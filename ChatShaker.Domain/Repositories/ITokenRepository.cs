using ChatShaker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Repositories
{
    public interface ITokenRepository
    {
        Task Add(Token token, CancellationToken cancellationToken);
        Task<Token?> GetTokenDataWithUser(string token, CancellationToken cancellationToken);
        Task DeleteToken(Token token, CancellationToken cancellationToken);

        Task<Token?> GetActualActivationTokenForUser(string token, string email, CancellationToken cancellationToken);
        Task DeleteExpiredTokens(CancellationToken cancellationToken);
    }
}
