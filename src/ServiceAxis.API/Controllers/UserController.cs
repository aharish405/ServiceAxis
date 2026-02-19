using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Domain.Common;

namespace ServiceAxis.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class UserController : BaseApiController
{
    private readonly IIdentityService _identity;

    public UserController(IIdentityService identity)
    {
        _identity = identity;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 20)
    {
        var result = await _identity.GetUsersAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var result = await _identity.GetUserAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateRoles(string id, [FromBody] IEnumerable<string> roles)
    {
        await _identity.UpdateUserRolesAsync(id, roles);
        return NoContent();
    }

    [HttpPut("{id}/profile")]
    public async Task<IActionResult> UpdateProfile(string id, UpdateProfileRequest request)
    {
        await _identity.UpdateUserProfileAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateUser(string id)
    {
        await _identity.DeactivateUserAsync(id);
        return NoContent();
    }
}
