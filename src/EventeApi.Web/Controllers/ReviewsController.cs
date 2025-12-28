using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventeApi.Web.Services;
using Refit;

namespace EventeApi.Web.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class ReviewsController : Controller
{
    private readonly IBackendApi _backendApi;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IBackendApi backendApi, ILogger<ReviewsController> logger)
    {
        _backendApi = backendApi;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? eventId)
    {
        try
        {
            if (!eventId.HasValue)
            {
                // Load all events to allow filtering
                var eventsResponse = await _backendApi.GetEventsAsync();
                if (eventsResponse.IsSuccessStatusCode && eventsResponse.Content != null)
                {
                    ViewBag.Events = eventsResponse.Content;
                }
                return View();
            }

            var response = await _backendApi.GetEventReviewsAsync(eventId.Value);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                ViewBag.EventId = eventId;
                return View(response.Content);
            }

            TempData["Error"] = "Failed to load reviews.";
            return View();
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while fetching reviews for event {EventId}", eventId);
            TempData["Error"] = "Failed to load reviews.";
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching reviews for event {EventId}", eventId);
            TempData["Error"] = "An unexpected error occurred.";
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        try
        {
            var response = await _backendApi.DeleteReviewAsync(id);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Review deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete review.";
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while deleting review {ReviewId}", id);
            TempData["Error"] = "Failed to delete review.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting review {ReviewId}", id);
            TempData["Error"] = "An unexpected error occurred.";
        }

        return RedirectToAction(nameof(Index), new { eventId });
    }
}

