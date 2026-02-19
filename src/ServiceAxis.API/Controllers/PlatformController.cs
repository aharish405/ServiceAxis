using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Features.Platform.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Platform API — user context, permissions, and platform health statistics.
/// </summary>
[Authorize]
public class PlatformController : BaseApiController
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permission;
    private readonly IMediator _mediator;

    public PlatformController(ICurrentUserService currentUser, IPermissionService permission, IMediator mediator)
    {
        _currentUser = currentUser;
        _permission  = permission;
        _mediator    = mediator;
    }

    /// <summary>Returns platform context for the authenticated user.</summary>
    [HttpGet("me")]
    [ProducesResponseType(200)]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            UserId        = _currentUser.UserId,
            Email         = _currentUser.Email,
            TenantId      = _currentUser.TenantId,
            Roles         = _currentUser.Roles,
            CorrelationId = _currentUser.CorrelationId,
            Timestamp     = DateTime.UtcNow,
            Platform      = "ServiceAxis v1.0"
        });
    }

    /// <summary>Returns all functional permissions granted to the current user.</summary>
    [HttpGet("permissions")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await _permission.GetUserPermissionsAsync();
        return Ok(permissions);
    }

    /// <summary>Returns live platform health statistics (record count, SLA, workflow metrics).</summary>
    [HttpGet("stats")]
    [Authorize(Policy = "AgentUp")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _mediator.Send(new GetPlatformStatsQuery(), ct);
        return Ok(stats);
    }

    /// <summary>Admin-only endpoint with full platform status.</summary>
    [HttpGet("admin-dashboard")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> AdminDashboard(CancellationToken ct)
    {
        var stats = await _mediator.Send(new GetPlatformStatsQuery(), ct);
        return Ok(new
        {
            Message   = "Welcome to the ServiceAxis Admin Dashboard.",
            Stats     = stats,
            Timestamp = DateTime.UtcNow
        });
    }
}
