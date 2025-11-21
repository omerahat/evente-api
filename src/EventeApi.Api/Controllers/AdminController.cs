using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventeApi.Core.DTOs;
using EventeApi.Core.Interfaces;

namespace EventeApi.Api.Controllers;

[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBadgeService _badgeService;

    public AdminController(IUserService userService, IBadgeService badgeService)
    {
        _userService = userService;
        _badgeService = badgeService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardMetricsDto>> GetDashboard()
    {
        var metrics = await _userService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost("users/{userId}/ban")]
    public async Task<IActionResult> BanUser(int userId)
    {
        var success = await _userService.BanUserAsync(userId);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("users/{userId}/unban")]
    public async Task<IActionResult> UnbanUser(int userId)
    {
        var success = await _userService.UnbanUserAsync(userId);
        if (!success) return NotFound();
        return NoContent();
    }

    // Badge Management
    [HttpGet("badges")]
    public async Task<ActionResult<IEnumerable<BadgeDto>>> GetBadges()
    {
        var badges = await _badgeService.GetAllBadgesAsync();
        return Ok(badges);
    }

    [HttpPost("badges")]
    public async Task<ActionResult<BadgeDto>> CreateBadge([FromBody] CreateBadgeDto dto)
    {
        var badge = await _badgeService.CreateBadgeAsync(dto);
        return CreatedAtAction(nameof(GetBadges), new { id = badge.Id }, badge);
    }

    [HttpPost("badges/assign")]
    public async Task<IActionResult> AssignBadge([FromBody] AssignBadgeDto dto)
    {
        var success = await _badgeService.AssignBadgeToUserAsync(dto.UserId, dto.BadgeId);
        if (!success) return BadRequest(new { Message = "Badge already assigned or invalid IDs." });
        return Ok(new { Message = "Badge assigned successfully." });
    }
}

