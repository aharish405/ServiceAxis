using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Identity;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Example secured endpoint that demonstrates JWT authentication and role-based authorization.
/// </summary>
[Authorize]
public class PlatformController : BaseApiController
{
    private readonly ICurrentUserService _currentUser;

    public PlatformController(ICurrentUserService currentUser) => _currentUser = currentUser;

    /// <summary>
    /// Returns platform information for the authenticated user.
    /// Requires any authenticated user.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(200)]
    public IActionResult GetCurrentUser()
    {
        var info = new
        {
            UserId        = _currentUser.UserId,
            Email         = _currentUser.Email,
            CorrelationId = _currentUser.CorrelationId,
            Timestamp     = DateTime.UtcNow,
            Platform      = "ServiceAxis v1.0"
        };
        return Ok(info);
    }

    /// <summary>
    /// Admin-only endpoint demonstrating policy-based authorization.
    /// </summary>
    [HttpGet("admin-dashboard")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public IActionResult AdminDashboard()
    {
        return Ok(new
        {
            Message   = "Welcome to the ServiceAxis Admin Dashboard.",
            Timestamp = DateTime.UtcNow
        });
    }
}
