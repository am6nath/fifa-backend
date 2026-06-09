using System.Net;

namespace fifa_backend.Exceptions;

/// <summary>
/// Exception thrown when client request is invalid (maps to HTTP 400).
/// </summary>
public class BadRequestException : ApiException
{
    public BadRequestException(string message, List<string>? errors = null)
        : base(message, HttpStatusCode.BadRequest, errors)
    {
    }
}
