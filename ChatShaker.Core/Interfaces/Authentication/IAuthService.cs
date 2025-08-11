using ChatShaker.Core.Models.Authentication;
using ChatShaker.Core.Models.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Core.Interfaces.Authentication
{
    public interface IAuthService
    {
        Task<AuthTokenModel> GenerateJwtToken(UserModel model, CancellationToken cancellationToken);
    }
}
