using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventeApi.Core.DTOs;
using EventeApi.Infrastructure;

namespace EventeApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();
        
        return Ok(categories);
    }
}
