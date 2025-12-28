using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventeApi.Core.DTOs;
using EventeApi.Web.Services;
using Refit;

namespace EventeApi.Web.Controllers;

public class AuthController : Controller
{
    private readonly IBackendApi _backendApi;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IBackendApi backendApi, ILogger<AuthController> logger)
    {
        _backendApi = backendApi;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _backendApi.LoginAsync(model);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                // Extract token from response
                var token = response.Content.Token;

                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve authentication token.");
                    return View(model);
                }

                // Decode JWT token to extract claims
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email)?.Value;
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError(string.Empty, "Invalid token structure.");
                    return View(model);
                }

                // Create claims for cookie authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email ?? string.Empty),
                    new Claim(ClaimTypes.Name, email ?? string.Empty),
                    new Claim("access_token", token)
                };

                if (!string.IsNullOrEmpty(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    });

                _logger.LogInformation("User {Email} logged in successfully", email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "API error during login");
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out");
        return RedirectToAction("Login", "Auth");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

