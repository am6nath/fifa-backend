namespace fifa_backend.DTOs.Common;

/// <summary>
/// Standardized API response envelope used by every endpoint.
/// Ensures the Angular frontend has a single, predictable contract
/// regardless of which endpoint is called.
///
/// Shape:
/// {
///   "success": true/false,
///   "statusCode": 200,
///   "message": "...",
///   "data": { ... },
///   "errors": ["..."],
///   "traceId": "correlation-id"
/// }
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? TraceId { get; set; }

    /// <summary>
    /// Factory for successful responses.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T? data, string message = "Request successful", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Factory for error responses.
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

/// <summary>
/// Non-generic version for endpoints that return no data payload.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Request successful")
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = 200,
            Message = message
        };
    }

    public static ApiResponse Fail(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
