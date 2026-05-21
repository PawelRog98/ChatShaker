using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatShaker.Domain.Enums;

namespace ChatShaker.Domain.Serivces
{
    public interface IAuthService
    {
        Task<AuthTokenModel> GenerateJwtToken(User user, CancellationToken cancellationToken);

        Task<Token> CreateToken(long userId, TokenType tokenType, DateTime expireDateUtc, string tokenData,
            CancellationToken cancellationToken);
    }
}
