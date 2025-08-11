using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Application.Users.Commands.Shared
{
    public class AuthTokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserNick { get; set; }
    }
}
