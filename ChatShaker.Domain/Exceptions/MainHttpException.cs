using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Exceptions
{
    public abstract class MainHttpException : Exception
    {
        public virtual int StatusCode { get; set; }
        protected MainHttpException(string? message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
