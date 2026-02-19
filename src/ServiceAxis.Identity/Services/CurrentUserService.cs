using Microsoft.AspNetCore.Http;
using ServiceAxis.Application.Contracts.Identity;
using System.Security.Claims;

namespace ServiceAxis.Identity.Services;

/// <summary>
/// Extracts the current user's identity from the HTTP context.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User?.FindFirstValue("sub");

    /// <inheritdoc />
    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
                          ?? User?.FindFirstValue("email");

    /// <inheritdoc />
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc />
    public string? CorrelationId =>
        _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? _httpContextAccessor.HttpContext?.TraceIdentifier;

    /// <inheritdoc />
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
