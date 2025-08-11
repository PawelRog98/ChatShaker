using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Infrastructure.Configuration
{
    public class JwtSettings
    {
        public string JwtKey { get; set; }
        public int JwtExpireMinutes { get; set; }
        public string JwtIssuer { get; set; }
    }
}
