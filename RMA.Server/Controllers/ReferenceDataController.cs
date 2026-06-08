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
    private readonly FirestoreRepository<Vendor> _vendorRepo;
    private readonly FirestoreRepository<Model> _modelRepo;
    private readonly FirestoreRepository<Category> _categoryRepo;

    public ReferenceDataController(
        FirestoreRepository<Vendor> vendorRepo, 
        FirestoreRepository<Model> modelRepo,
        FirestoreRepository<Category> categoryRepo)
    {
        _vendorRepo = vendorRepo;
        _modelRepo = modelRepo;
        _categoryRepo = categoryRepo;
    }

    [HttpGet("vendors")]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetVendors()
    {
        var vendors = await _vendorRepo.GetAllAsync();
        if (!vendors.Any())
        {
            var defaults = new List<Vendor>
            {
                new() { Name = "Apple Service" },
                new() { Name = "Dell Service" },
                new() { Name = "Asus Service" },
                new() { Name = "HP Service" }
            };
            foreach (var v in defaults)
            {
                v.Id = await _vendorRepo.AddAsync(v);
            }
            vendors = defaults;
        }

        var dtos = vendors.Select(v => new VendorDto
        {
            Id = v.Id,
            Name = v.Name
        });
        return Ok(dtos);
    }

    [HttpPost("vendors")]
    public async Task<ActionResult<VendorDto>> PostVendor([FromBody] VendorDto dto)
    {
        var entity = new Vendor { Name = dto.Name };
        var newId = await _vendorRepo.AddAsync(entity);
        return CreatedAtAction(nameof(GetVendors), null, new VendorDto { Id = newId, Name = entity.Name });
    }

    [HttpPut("vendors/{id}")]
    public async Task<IActionResult> PutVendor(string id, [FromBody] VendorDto dto)
    {
        var entity = await _vendorRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        await _vendorRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("vendors/{id}")]
    public async Task<IActionResult> DeleteVendor(string id)
    {
        await _vendorRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("models")]
    public async Task<ActionResult<IEnumerable<ModelDto>>> GetModels()
    {
        var models = await _modelRepo.GetAllAsync();
        if (!models.Any())
        {
            var defaults = new List<Model>
            {
                new() { ModelName = "Dell XPS 15", Brand = "Dell", CategoryId = "1" },
                new() { ModelName = "MacBook Pro 14", Brand = "Apple", CategoryId = "1" },
                new() { ModelName = "Asus ROG G14", Brand = "Asus", CategoryId = "1" }
            };
            foreach (var m in defaults)
            {
                m.Id = await _modelRepo.AddAsync(m);
            }
            models = defaults;
        }

        var dtos = models.Select(m => new ModelDto
        {
            Id = m.Id,
            ModelName = m.ModelName,
            Brand = m.Brand,
            CategoryId = m.CategoryId
        });
        return Ok(dtos);
    }

    [HttpPost("models")]
    public async Task<ActionResult<ModelDto>> PostModel([FromBody] ModelDto dto)
    {
        var entity = new Model
        {
            ModelName = dto.ModelName,
            Brand = dto.Brand,
            CategoryId = string.IsNullOrEmpty(dto.CategoryId) ? "1" : dto.CategoryId
        };
        var newId = await _modelRepo.AddAsync(entity);
        return CreatedAtAction(nameof(GetModels), null, new ModelDto
        {
            Id = newId,
            ModelName = entity.ModelName,
            Brand = entity.Brand,
            CategoryId = entity.CategoryId
        });
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
}
