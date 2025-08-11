using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Exceptions
{
    public class SuspendedUserException : Exception
    {
        public SuspendedUserException(DateTime suspendDate) : base($"User is suspended until: {suspendDate.ToString("yyyy-MM-dd HH:mm:ss")}") { }
    }
}
