using Microsoft.AspNetCore.Mvc;
using EventeApi.Core.DTOs;
using EventeApi.Core.Interfaces;

namespace EventeApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto.Email, dto.Password, dto.FullName);
            return Ok(new { user.Id, user.Email, Message = "Registration successful" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var token = await _authService.LoginAsync(dto.Email, dto.Password);

            if (token == null)
            {
                return Unauthorized(new { Error = "Invalid email or password" });
            }

            return Ok(new { Token = token });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}

