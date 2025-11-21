using Microsoft.AspNetCore.Mvc;

namespace EventeApi.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Detail = ex.Message // In production, consider hiding this
        };

        if (ex is InvalidOperationException || ex is ArgumentException)
        {
            response.Status = StatusCodes.Status400BadRequest;
            response.Title = "Bad Request";
        }
        else if (ex is KeyNotFoundException)
        {
            response.Status = StatusCodes.Status404NotFound;
            response.Title = "Not Found";
        }
        else if (ex is UnauthorizedAccessException)
        {
            response.Status = StatusCodes.Status401Unauthorized;
            response.Title = "Unauthorized";
        }

        context.Response.StatusCode = response.Status.Value;
        return context.Response.WriteAsJsonAsync(response);
    }
}

