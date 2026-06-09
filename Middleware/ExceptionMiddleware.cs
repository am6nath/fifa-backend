using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using fifa_backend.DTOs.Common;
using fifa_backend.Exceptions;

namespace fifa_backend.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions,
/// logs them with correlation context, and returns a standardized ApiResponse.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";
        List<string>? errors = null;

        switch (exception)
        {
            case ApiException apiException:
                statusCode = apiException.StatusCode;
                message = apiException.Message;
                errors = apiException.Errors;
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                break;
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "The requested resource was not found.";
                break;
            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                message = argEx.Message;
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        // Try to get Correlation ID
        string? correlationId = null;
        if (context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdHeaderKey, out var cId))
        {
            correlationId = cId?.ToString();
        }

        var response = new ApiResponse<object>
        {
            Success = false,
            StatusCode = (int)statusCode,
            Message = message,
            Errors = errors ?? new List<string>(),
            TraceId = correlationId
        };

        // If we are in Development and it's a 500 error, output the stack trace inside the errors list for convenience.
        if (statusCode == HttpStatusCode.InternalServerError && _env.IsDevelopment())
        {
            response.Errors.Add(exception.ToString());
        }

        var options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}
