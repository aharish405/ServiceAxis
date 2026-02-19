using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ServiceAxis.API.Middleware;

/// <summary>
/// Middleware that logs each HTTP request/response with timing and correlation ID support.
/// Injects an X-Correlation-Id header if not already present in the incoming request.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ensure correlation ID
        if (!context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers["X-Correlation-Id"] = correlationId;
        }

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "→ {Method} {Path} | CorrelationId={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "← {Method} {Path} | {StatusCode} | {Elapsed}ms | CorrelationId={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            correlationId);
    }
}
