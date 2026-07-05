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
public class ModelsController : ControllerBase
{
    private readonly FirestoreRepository<Model> _modelRepo;
    private readonly FirestoreRepository<Brand> _brandRepo;

    public ModelsController(FirestoreRepository<Model> modelRepo, FirestoreRepository<Brand> brandRepo)
    {
        _modelRepo = modelRepo;
        _brandRepo = brandRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModelDto>>> GetModels()
    {
        var models = await _modelRepo.GetAllAsync();
        
        // Auto seed
        if (!models.Any())
        {
            // Seed a default brand first if needed to get an ID
            var brands = await _brandRepo.GetAllAsync();
            var appleBrand = brands.FirstOrDefault(b => b.Name == "Apple")?.Id;
            var dellBrand = brands.FirstOrDefault(b => b.Name == "Dell")?.Id;
            var asusBrand = brands.FirstOrDefault(b => b.Name == "Asus")?.Id;

            var defaults = new List<Model>
            {
                new() { ModelName = "Dell XPS 15", BrandId = dellBrand, CategoryId = "1", IsSerialRequired = true },
                new() { ModelName = "MacBook Pro 14", BrandId = appleBrand, CategoryId = "1", IsSerialRequired = true },
                new() { ModelName = "Asus ROG G14", BrandId = asusBrand, CategoryId = "1", IsSerialRequired = true }
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
            BrandId = m.BrandId,
            CategoryId = m.CategoryId,
            StockQuantity = m.StockQuantity,
            WarrantyMonths = m.WarrantyMonths,
            IsSerialRequired = m.IsSerialRequired
        });
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<ModelDto>> PostModel([FromBody] ModelDto dto)
    {
        var entity = new Model
        {
            ModelName = dto.ModelName,
            BrandId = dto.BrandId,
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
            BrandId = entity.BrandId,
            CategoryId = entity.CategoryId,
            StockQuantity = entity.StockQuantity,
            WarrantyMonths = entity.WarrantyMonths,
            IsSerialRequired = entity.IsSerialRequired
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutModel(string id, [FromBody] ModelDto dto)
    {
        var entity = await _modelRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.ModelName = dto.ModelName;
        entity.BrandId = dto.BrandId;
        entity.CategoryId = string.IsNullOrEmpty(dto.CategoryId) ? "1" : dto.CategoryId;
        entity.IsSerialRequired = dto.IsSerialRequired;
        entity.StockQuantity = dto.StockQuantity;
        entity.WarrantyMonths = dto.WarrantyMonths;

        await _modelRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteModel(string id)
    {
        await _modelRepo.DeleteAsync(id);
        return NoContent();
    }
}
