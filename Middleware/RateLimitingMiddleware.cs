using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using fifa_backend.DTOs.Common;

namespace fifa_backend.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, List<DateTime>> _requestStore = new();

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        // Define rate limits: (MaxRequests, PeriodMinutes)
        (int MaxRequests, int PeriodMinutes)? limit = null;

        if (path == "/api/v1/auth/send-otp")
        {
            limit = (5, 1); // 5 requests per minute
        }
        else if (path == "/api/v1/auth/verify-otp" || path == "/api/v1/auth/admin-login")
        {
            limit = (10, 1); // 10 requests per minute
        }

        if (limit.HasValue)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"{clientIp}:{path}";
            var now = DateTime.UtcNow;

            var requests = _requestStore.GetOrAdd(key, _ => new List<DateTime>());

            lock (requests)
            {
                // Remove expired timestamps
                var periodStart = now.AddMinutes(-limit.Value.PeriodMinutes);
                requests.RemoveAll(t => t < periodStart);

                if (requests.Count >= limit.Value.MaxRequests)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/json";

                    var response = new ApiResponse<object>
                    {
                        Success = false,
                        StatusCode = (int)HttpStatusCode.TooManyRequests,
                        Message = "Too many requests. Please try again later.",
                        Errors = new List<string> { "Rate limit exceeded. OTP and login requests are limited to prevent spam." }
                    };

                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    context.Response.WriteAsync(json).Wait();
                    return;
                }

                requests.Add(now);
            }
        }

        await _next(context);
    }
}
