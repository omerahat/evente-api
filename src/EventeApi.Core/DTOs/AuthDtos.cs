namespace EventeApi.Core.DTOs;

public record RegisterDto(string Email, string Password, string FullName);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(string Token, int UserId, string Email, string Role);

