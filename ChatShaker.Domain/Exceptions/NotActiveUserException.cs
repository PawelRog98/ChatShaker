using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Exceptions
{
    public class NotActiveUserException : Exception
    {
        public NotActiveUserException() : base("User is not active") { }
    }
}
