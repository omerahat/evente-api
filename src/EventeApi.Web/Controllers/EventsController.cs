using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventeApi.Core.DTOs;
using EventeApi.Web.Services;
using Refit;

namespace EventeApi.Web.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class EventsController : Controller
{
    private readonly IBackendApi _backendApi;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IBackendApi backendApi, ILogger<EventsController> logger)
    {
        _backendApi = backendApi;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _backendApi.GetEventsAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return View(response.Content);
            }

            TempData["Error"] = "Failed to load events.";
            return View();
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while fetching events");
            TempData["Error"] = "Failed to load events.";
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching events");
            TempData["Error"] = "An unexpected error occurred.";
            return View();
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var response = await _backendApi.GetEventByIdAsync(id);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return View(response.Content);
            }

            TempData["Error"] = "Event not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while fetching event {EventId}", id);
            TempData["Error"] = "Failed to load event details.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching event {EventId}", id);
            TempData["Error"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _backendApi.CreateEventAsync(model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to create event.";
            return View(model);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while creating event");
            TempData["Error"] = "Failed to create event.";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating event");
            TempData["Error"] = "An unexpected error occurred.";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var response = await _backendApi.GetEventByIdAsync(id);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return View(response.Content);
            }

            TempData["Error"] = "Event not found.";
            return RedirectToAction(nameof(Index));
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while fetching event for edit {EventId}", id);
            TempData["Error"] = "Failed to load event for editing.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching event for edit {EventId}", id);
            TempData["Error"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateEventDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _backendApi.UpdateEventAsync(id, model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to update event.";
            return View(model);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while updating event {EventId}", id);
            TempData["Error"] = "Failed to update event.";
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating event {EventId}", id);
            TempData["Error"] = "An unexpected error occurred.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var response = await _backendApi.DeleteEventAsync(id);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete event.";
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while deleting event {EventId}", id);
            TempData["Error"] = "Failed to delete event.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting event {EventId}", id);
            TempData["Error"] = "An unexpected error occurred.";
        }

        return RedirectToAction(nameof(Index));
    }
}

