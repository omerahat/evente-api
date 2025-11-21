using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RteApi.Core.Interfaces;

namespace RteApi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost("{eventId}")]
    public async Task<IActionResult> Register(int eventId)
    {
        var userId = GetUserId();
        try
        {
            var result = await _registrationService.RegisterUserAsync(userId, eventId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> Unregister(int eventId)
    {
        var userId = GetUserId();
        var success = await _registrationService.UnregisterUserAsync(userId, eventId);
        if (!success) return NotFound(new { Message = "Registration not found" });
        return NoContent();
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRegistrations()
    {
        var userId = GetUserId();
        var registrations = await _registrationService.GetUserRegistrationsAsync(userId);
        return Ok(registrations);
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }
}

