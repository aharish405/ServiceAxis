using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ServiceAxis.Shared.Exceptions;
using ServiceAxis.Shared.Wrappers;
using System.Net;
using System.Text.Json;

namespace ServiceAxis.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions, maps them to appropriate HTTP status codes,
/// and returns a standardised <see cref="ApiResponse{T}"/> JSON body.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException ex      => (HttpStatusCode.NotFound,            ex.Message, Array.Empty<string>()),
            BusinessException ex      => (HttpStatusCode.UnprocessableEntity,  ex.Message, Array.Empty<string>()),
            ForbiddenException ex     => (HttpStatusCode.Forbidden,            ex.Message, Array.Empty<string>()),
            ConflictException ex      => (HttpStatusCode.Conflict,             ex.Message, Array.Empty<string>()),
            ValidationException ex    => (HttpStatusCode.BadRequest,           "One or more validation errors occurred.",
                                         ex.Errors.Select(e => e.ErrorMessage).ToArray()),
            _                         => (HttpStatusCode.InternalServerError,  "An unexpected error occurred. Please try again later.",
                                         Array.Empty<string>())
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, errors);
        var json     = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
