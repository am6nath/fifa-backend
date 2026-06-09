using System.Net;

namespace fifa_backend.Exceptions;

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message)
        : base(message, HttpStatusCode.Forbidden)
    {
    }
}
