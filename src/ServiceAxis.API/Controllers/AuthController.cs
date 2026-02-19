using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Identity;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Authentication controller — login, register, refresh, and revoke endpoints.
/// </summary>
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>
    /// Authenticates a user and returns a JWT access token + refresh token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(result, "Login successful.");
    }

    /// <summary>
    /// Registers a new platform user and returns JWT tokens.
    /// </summary>
    /// <param name="request">Registration details.</param>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return Created(result, "Registration successful.");
    }

    /// <summary>
    /// Issues a new access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
        return Ok(result, "Token refreshed.");
    }

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    [HttpPost("revoke")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken, ct);
        return Ok<object>(null!, "Token revoked.");
    }
}

/// <summary>Request record for refresh/revoke operations.</summary>
public record RefreshTokenRequest(string RefreshToken);
