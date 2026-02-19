using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Shared.Wrappers;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Base controller providing the standard API response envelope and common functionality
/// for all platform controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>Returns HTTP 200 with a typed success response.</summary>
    protected IActionResult Ok<T>(T data, string message = "Success") =>
        base.Ok(ApiResponse<T>.Success(data, message));

    /// <summary>Returns HTTP 201 Created with a typed success response.</summary>
    protected IActionResult Created<T>(T data, string message = "Resource created successfully.") =>
        StatusCode(201, ApiResponse<T>.Success(data, message));

    /// <summary>Returns HTTP 400 with a failure response.</summary>
    protected IActionResult BadRequest(string message, IEnumerable<string>? errors = null) =>
        base.BadRequest(ApiResponse<object>.Fail(message, errors));

    /// <summary>Returns HTTP 404 with a failure response.</summary>
    protected IActionResult NotFound(string message) =>
        base.NotFound(ApiResponse<object>.Fail(message));
}
