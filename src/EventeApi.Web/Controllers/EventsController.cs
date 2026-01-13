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
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventDto model)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
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
            await LoadCategoriesAsync();
            return View(model);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while creating event");
            TempData["Error"] = "Failed to create event.";
            await LoadCategoriesAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating event");
            TempData["Error"] = "An unexpected error occurred.";
            await LoadCategoriesAsync();
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
                await LoadCategoriesAsync();
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
            await LoadCategoriesAsync();
            // Fetch the current event to convert UpdateEventDto to EventDto for the view
            var eventResponse = await _backendApi.GetEventByIdAsync(id);
            if (eventResponse.IsSuccessStatusCode && eventResponse.Content != null)
            {
                var eventDto = eventResponse.Content;
                var updatedEventDto = new EventDto(
                    eventDto.Id,
                    model.Title ?? eventDto.Title,
                    model.Description ?? eventDto.Description,
                    model.OrganizerName ?? eventDto.OrganizerName,
                    model.EventTime ?? eventDto.EventTime,
                    model.LocationName ?? eventDto.LocationName,
                    model.LocationLat ?? eventDto.LocationLat,
                    model.LocationLon ?? eventDto.LocationLon,
                    model.CategoryId ?? eventDto.CategoryId,
                    eventDto.CategoryName,
                    eventDto.CreatedByAdminId,
                    eventDto.CreatedAt,
                    model.BannerImageUrl ?? eventDto.BannerImageUrl
                );
                return View(updatedEventDto);
            }
            // If we can't fetch the event, redirect to index
            TempData["Error"] = "Failed to load event for editing.";
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while updating event {EventId}", id);
            TempData["Error"] = "Failed to update event.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating event {EventId}", id);
            TempData["Error"] = "An unexpected error occurred.";
            return RedirectToAction(nameof(Edit), new { id });
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

    /// <summary>
    /// Upload an event banner image via AJAX
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, errorMessage = "No file provided." });
        }

        try
        {
            // Convert IFormFile to StreamPart for Refit
            using var stream = file.OpenReadStream();
            var streamPart = new StreamPart(stream, file.FileName, file.ContentType);
            
            var response = await _backendApi.UploadEventImageAsync(streamPart);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return Json(new 
                { 
                    success = response.Content.Success, 
                    url = response.Content.Url,
                    fileName = response.Content.FileName,
                    fileSize = response.Content.FileSize,
                    errorMessage = response.Content.ErrorMessage
                });
            }

            return Json(new { success = false, errorMessage = "Upload failed. Please try again." });
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while uploading image");
            return Json(new { success = false, errorMessage = "Upload failed. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while uploading image");
            return Json(new { success = false, errorMessage = "An unexpected error occurred." });
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await _backendApi.GetCategoriesAsync();
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                ViewBag.Categories = response.Content;
            }
            else
            {
                ViewBag.Categories = new List<CategoryDto>();
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while fetching categories");
            ViewBag.Categories = new List<CategoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching categories");
            ViewBag.Categories = new List<CategoryDto>();
        }
    }
}

