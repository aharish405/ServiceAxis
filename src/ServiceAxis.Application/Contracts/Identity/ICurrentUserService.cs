namespace ServiceAxis.Application.Contracts.Identity;

/// <summary>
/// Provides information about the currently authenticated user.
/// Implemented by the API layer via IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Gets the authenticated user's identity ID.</summary>
    string? UserId { get; }

    /// <summary>Gets the authenticated user's email.</summary>
    string? Email { get; }

    /// <summary>Gets a value indicating whether the request is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets the correlation ID for the current request.</summary>
    string? CorrelationId { get; }

    /// <summary>Returns true if the current user is in the given role.</summary>
    bool IsInRole(string role);
}
