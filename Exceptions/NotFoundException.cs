using System.Net;

namespace fifa_backend.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found (maps to HTTP 404).
/// </summary>
public class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(message, HttpStatusCode.NotFound)
    {
    }
}
