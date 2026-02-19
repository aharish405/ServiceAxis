namespace ServiceAxis.Application.Contracts.Identity;

/// <summary>
/// Request model for user login.
/// </summary>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Response returned after a successful authentication.
/// </summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    IEnumerable<string> Roles);

/// <summary>
/// Request model for user registration.
/// </summary>
public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber = null);

/// <summary>
/// Contract for the identity / authentication service.
/// </summary>
public interface IAuthService
{
    /// <summary>Authenticates a user and returns a JWT token pair.</summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Registers a new platform user.</summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Issues a new access token using a valid refresh token.</summary>
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes the given refresh token.</summary>
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
