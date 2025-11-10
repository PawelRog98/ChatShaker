using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Serivces
{
    public interface IAuthService
    {
        Task<AuthTokenModel> GenerateJwtToken(User user, CancellationToken cancellationToken);
    }
}
