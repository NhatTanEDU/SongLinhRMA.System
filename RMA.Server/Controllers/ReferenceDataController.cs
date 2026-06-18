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
                new() { ModelName = "Dell XPS 15", Brand = "Dell", CategoryId = "1", IsSerialRequired = true },
                new() { ModelName = "MacBook Pro 14", Brand = "Apple", CategoryId = "1", IsSerialRequired = true },
                new() { ModelName = "Asus ROG G14", Brand = "Asus", CategoryId = "1", IsSerialRequired = true }
            };
            foreach (var m in defaults)
            {
                m.Id = await _modelRepo.AddAsync(m);
            }
            models = defaults;
        }

        // Auto-fix: Ensure Access Point and other hardware models have IsSerialRequired set to true in Firestore
        bool updated = false;
        foreach (var m in models)
        {
            var lowerName = m.ModelName.ToLower();
            if ((lowerName.Contains("access point") || lowerName.Contains("laptop") || lowerName.Contains("ups") || lowerName.Contains("switch") || lowerName.Contains("dell") || lowerName.Contains("macbook") || lowerName.Contains("asus")) && !m.IsSerialRequired)
            {
                m.IsSerialRequired = true;
                await _modelRepo.UpdateAsync(m.Id, m);
                updated = true;
            }
        }
        if (updated)
        {
            models = await _modelRepo.GetAllAsync();
        }

        var dtos = models.Select(m => new ModelDto
        {
            Id = m.Id,
            ModelName = m.ModelName,
            Brand = m.Brand,
            CategoryId = m.CategoryId,
            StockQuantity = m.StockQuantity,
            WarrantyMonths = m.WarrantyMonths,
            IsSerialRequired = m.IsSerialRequired
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
            CategoryId = string.IsNullOrEmpty(dto.CategoryId) ? "1" : dto.CategoryId,
            IsSerialRequired = dto.IsSerialRequired,
            StockQuantity = dto.StockQuantity,
            WarrantyMonths = dto.WarrantyMonths
        };
        var newId = await _modelRepo.AddAsync(entity);
        return CreatedAtAction(nameof(GetModels), null, new ModelDto
        {
            Id = newId,
            ModelName = entity.ModelName,
            Brand = entity.Brand,
            CategoryId = entity.CategoryId,
            StockQuantity = entity.StockQuantity,
            WarrantyMonths = entity.WarrantyMonths,
            IsSerialRequired = entity.IsSerialRequired
        });
    }

    [HttpPut("models/{id}")]
    public async Task<IActionResult> PutModel(string id, [FromBody] ModelDto dto)
    {
        var entity = await _modelRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.ModelName = dto.ModelName;
        entity.Brand = dto.Brand;
        entity.CategoryId = string.IsNullOrEmpty(dto.CategoryId) ? "1" : dto.CategoryId;
        entity.IsSerialRequired = dto.IsSerialRequired;
        entity.StockQuantity = dto.StockQuantity;
        entity.WarrantyMonths = dto.WarrantyMonths;

        await _modelRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("models/{id}")]
    public async Task<IActionResult> DeleteModel(string id)
    {
        await _modelRepo.DeleteAsync(id);
        return NoContent();
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
