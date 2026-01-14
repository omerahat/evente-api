using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventeApi.Core.DTOs;
using EventeApi.Core.Interfaces;

namespace EventeApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll([FromQuery] int? categoryId = null)
    {
        if (categoryId.HasValue)
        {
            var filteredEvents = await _eventService.GetEventsByCategoryAsync(categoryId.Value);
            return Ok(filteredEvents);
        }
        
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();
        return Ok(evt);
    }

    [Authorize(Policy = "Admin")]
    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventDto dto)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
        {
            return Unauthorized();
        }

        var createdEvent = await _eventService.CreateEventAsync(dto, adminId);
        return CreatedAtAction(nameof(GetById), new { id = createdEvent.Id }, createdEvent);
    }

    [Authorize(Policy = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<EventDto>> Update(int id, [FromBody] UpdateEventDto dto)
    {
        var updatedEvent = await _eventService.UpdateEventAsync(id, dto);
        if (updatedEvent == null) return NotFound();
        return Ok(updatedEvent);
    }

    [Authorize(Policy = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _eventService.DeleteEventAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}

