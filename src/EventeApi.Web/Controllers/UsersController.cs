using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventeApi.Web.Services;
using Refit;

namespace EventeApi.Web.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IBackendApi _backendApi;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IBackendApi backendApi, ILogger<UsersController> logger)
    {
        _backendApi = backendApi;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _backendApi.GetUsersAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return View(response.Content);
            }

            TempData["Error"] = "Failed to load users.";
            return View();
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while fetching users");
            TempData["Error"] = "Failed to load users.";
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching users");
            TempData["Error"] = "An unexpected error occurred.";
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ban(int id)
    {
        try
        {
            var response = await _backendApi.BanUserAsync(id);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "User banned successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to ban user.";
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while banning user {UserId}", id);
            TempData["Error"] = "Failed to ban user.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while banning user {UserId}", id);
            TempData["Error"] = "An unexpected error occurred.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unban(int id)
    {
        try
        {
            var response = await _backendApi.UnbanUserAsync(id);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "User unbanned successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to unban user.";
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error while unbanning user {UserId}", id);
            TempData["Error"] = "Failed to unban user.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while unbanning user {UserId}", id);
            TempData["Error"] = "An unexpected error occurred.";
        }

        return RedirectToAction(nameof(Index));
    }
}

