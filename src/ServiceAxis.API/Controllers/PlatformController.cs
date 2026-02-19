using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Application.Contracts.Infrastructure;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Example secured endpoint that demonstrates JWT authentication and role-based authorization.
/// </summary>
[Authorize]
public class PlatformController : BaseApiController
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permission;

    public PlatformController(ICurrentUserService currentUser, IPermissionService permission)
    {
        _currentUser = currentUser;
        _permission = permission;
    }

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
            TenantId      = _currentUser.TenantId,
            Roles         = _currentUser.Roles,
            CorrelationId = _currentUser.CorrelationId,
            Timestamp     = DateTime.UtcNow,
            Platform      = "ServiceAxis v1.0"
        };
        return Ok(info);
    }

    /// <summary>
    /// Returns all functional permissions granted to the current user.
    /// </summary>
    [HttpGet("permissions")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _permission.GetUserPermissionsAsync();
        return Ok(permissions);
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
