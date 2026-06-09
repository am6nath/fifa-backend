using System.Net;

namespace fifa_backend.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs in resource state, e.g., duplicate resources (maps to HTTP 409).
/// </summary>
public class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(message, HttpStatusCode.Conflict)
    {
    }
}
