using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMA.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReferenceDataController : ControllerBase
{
    private readonly FirestoreRepository<Category> _categoryRepo;

    public ReferenceDataController(FirestoreRepository<Category> categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryRepo.GetAllAsync();
        if (!categories.Any())
        {
            var defaults = new List<Category>
            {
                new() { Name = "Laptop" },
                new() { Name = "PC (Máy bộ)" },
                new() { Name = "UPS (Bộ lưu điện)" },
                new() { Name = "Printer (Máy in)" },
                new() { Name = "Monitor (Màn hình)" }
            };
            foreach (var c in defaults)
            {
                c.Id = await _categoryRepo.AddAsync(c);
            }
            categories = defaults;
        }

        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        });
        return Ok(dtos);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> PostCategory([FromBody] CategoryDto dto)
    {
        var entity = new Category { Name = dto.Name };
        var newId = await _categoryRepo.AddAsync(entity);
        return CreatedAtAction(nameof(GetCategories), null, new CategoryDto { Id = newId, Name = entity.Name });
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> PutCategory(string id, [FromBody] CategoryDto dto)
    {
        var entity = await _categoryRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        await _categoryRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        await _categoryRepo.DeleteAsync(id);
        return NoContent();
    }
}
