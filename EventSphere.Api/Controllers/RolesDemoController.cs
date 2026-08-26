using EventSphere.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventSphere.Api.Controllers;

/// <summary>
/// Demonstrates and verifies server-side role authorization. Access is enforced
/// here by the backend — the React UI hiding a section is never the security boundary.
/// Roles are cumulative upward: higher roles may reach lower areas.
/// </summary>
[ApiController]
[Route("api/demo")]
public class RolesDemoController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public() => Ok(ApiResponse.Ok("Public area — no authentication required."));

    [HttpGet("visitor")]
    [Authorize(Roles = $"{AppRoles.Visitor},{AppRoles.Participant},{AppRoles.Organizer},{AppRoles.Admin}")]
    public IActionResult Visitor() => Ok(ApiResponse.Ok("Visitor area — any authenticated user."));

    [HttpGet("participant")]
    [Authorize(Roles = $"{AppRoles.Participant},{AppRoles.Organizer},{AppRoles.Admin}")]
    public IActionResult Participant() => Ok(ApiResponse.Ok("Participant area."));

    [HttpGet("organizer")]
    [Authorize(Roles = $"{AppRoles.Organizer},{AppRoles.Admin}")]
    public IActionResult Organizer() => Ok(ApiResponse.Ok("Organizer area."));

    [HttpGet("admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult Admin() => Ok(ApiResponse.Ok("Admin area."));
}
