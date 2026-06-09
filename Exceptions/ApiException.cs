using System.Net;

namespace fifa_backend.Exceptions;

/// <summary>
/// Base exception class for api-specific errors.
/// Caught by ExceptionMiddleware to map to appropriate API responses.
/// </summary>
public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public List<string>? Errors { get; }

    public ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, List<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
