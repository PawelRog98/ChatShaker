using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Exceptions
{
    public sealed class BadAuthenticationException : MainHttpException
    {
        public BadAuthenticationException(string message) : base(message, StatusCodes.Status401Unauthorized) { }
    }
}
