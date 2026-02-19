using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Identity;

namespace ServiceAxis.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class TenantController : BaseApiController
{
    private readonly IIdentityService _identity;

    public TenantController(IIdentityService identity)
    {
        _identity = identity;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetTenants()
    {
        var result = await _identity.GetAllTenantsAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        var result = await _identity.GetTenantAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateTenant(CreateTenantRequest request)
    {
        var result = await _identity.CreateTenantAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateTenant(Guid id, UpdateTenantRequest request)
    {
        await _identity.UpdateTenantAsync(id, request);
        return NoContent();
    }
}
