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
    private readonly IImageUploadService _imageUploadService;

    public AdminController(IUserService userService, IBadgeService badgeService, IImageUploadService imageUploadService)
    {
        _userService = userService;
        _badgeService = badgeService;
        _imageUploadService = imageUploadService;
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

    // Image Upload Endpoints
    
    /// <summary>
    /// Upload an image for events
    /// </summary>
    /// <param name="file">Image file (JPG, PNG, GIF, WebP, max 5MB)</param>
    /// <returns>Upload result with image URL</returns>
    [HttpPost("upload/event-image")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB limit
    public async Task<ActionResult<ImageUploadResponseDto>> UploadEventImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ImageUploadResponseDto(false, null, null, 0, "No file provided."));
        }

        // Validate the image
        var validation = _imageUploadService.ValidateImage(file.FileName, file.ContentType, file.Length);
        if (!validation.IsValid)
        {
            return BadRequest(new ImageUploadResponseDto(false, null, null, 0, validation.ErrorMessage));
        }

        // Upload the image
        using var stream = file.OpenReadStream();
        var result = await _imageUploadService.UploadImageAsync(stream, file.FileName, file.ContentType, "events");

        if (!result.Success)
        {
            return BadRequest(new ImageUploadResponseDto(false, null, null, 0, result.ErrorMessage));
        }

        return Ok(new ImageUploadResponseDto(true, result.Url, result.FileName, result.FileSize));
    }

    /// <summary>
    /// Upload multiple images for events (batch upload)
    /// </summary>
    /// <param name="files">Image files (JPG, PNG, GIF, WebP, max 5MB each)</param>
    /// <returns>Upload results with image URLs</returns>
    [HttpPost("upload/event-images")]
    [RequestSizeLimit(25 * 1024 * 1024)] // 25MB total limit for batch
    public async Task<ActionResult<IEnumerable<ImageUploadResponseDto>>> UploadEventImages(IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new[] { new ImageUploadResponseDto(false, null, null, 0, "No files provided.") });
        }

        var results = new List<ImageUploadResponseDto>();

        foreach (var file in files)
        {
            // Validate each image
            var validation = _imageUploadService.ValidateImage(file.FileName, file.ContentType, file.Length);
            if (!validation.IsValid)
            {
                results.Add(new ImageUploadResponseDto(false, null, file.FileName, file.Length, validation.ErrorMessage));
                continue;
            }

            // Upload the image
            using var stream = file.OpenReadStream();
            var result = await _imageUploadService.UploadImageAsync(stream, file.FileName, file.ContentType, "events");

            results.Add(new ImageUploadResponseDto(result.Success, result.Url, result.FileName ?? file.FileName, result.FileSize, result.ErrorMessage));
        }

        return Ok(results);
    }

    /// <summary>
    /// Delete an uploaded image
    /// </summary>
    /// <param name="imageUrl">The URL of the image to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("upload/image")]
    public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return BadRequest(new { Message = "Image URL is required." });
        }

        var success = await _imageUploadService.DeleteImageAsync(imageUrl);
        if (!success)
        {
            return NotFound(new { Message = "Image not found or could not be deleted." });
        }

        return Ok(new { Message = "Image deleted successfully." });
    }
}

