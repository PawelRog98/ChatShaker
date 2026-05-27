using Microsoft.AspNetCore.Http;

namespace ChatShaker.Domain.Exceptions;

public sealed class ForbiddenException : MainHttpException
{
    public ForbiddenException(string message) : base(message, StatusCodes.Status403Forbidden){}
}
