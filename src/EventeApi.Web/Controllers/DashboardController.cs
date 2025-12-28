using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventeApi.Web.Services;
using Refit;

namespace EventeApi.Web.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IBackendApi _backendApi;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IBackendApi backendApi, ILogger<DashboardController> logger)
    {
        _backendApi = backendApi;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _backendApi.GetDashboardMetricsAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return View(response.Content);
            }

            TempData["Error"] = "Failed to load dashboard metrics.";
            return View();
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while fetching dashboard metrics");
            TempData["Error"] = "Failed to load dashboard metrics.";
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching dashboard metrics");
            TempData["Error"] = "An unexpected error occurred.";
            return View();
        }
    }
}

