using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace fifa_backend.Middleware;

/// <summary>
/// Middleware to assign and forward a unique Correlation ID for every request/response.
/// Used for request tracing across distributed systems/containers.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    public const string CorrelationIdHeaderKey = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out StringValues correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store the correlation ID in the request context items for access elsewhere
        context.Items[CorrelationIdHeaderKey] = correlationId.ToString();

        // Ensure the correlation ID is written back to the response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
            {
                context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);
            }
            return Task.CompletedTask;
        });

        // Set logging scope with Correlation ID
        var logger = context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>();
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId.ToString() }))
        {
            await _next(context);
        }
    }
}
