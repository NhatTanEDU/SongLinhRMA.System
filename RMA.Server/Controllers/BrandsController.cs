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
public class BrandsController : ControllerBase
{
    private readonly FirestoreRepository<Brand> _brandRepo;

    public BrandsController(FirestoreRepository<Brand> brandRepo)
    {
        _brandRepo = brandRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
    {
        var brands = await _brandRepo.GetAllAsync();
        
        // Auto seed
        if (!brands.Any())
        {
            var defaults = new List<Brand>
            {
                new() { Name = "Apple" },
                new() { Name = "Dell" },
                new() { Name = "Asus" },
                new() { Name = "HP" },
                new() { Name = "APC" },
                new() { Name = "Cisco" },
                new() { Name = "Santak" }
            };
            foreach (var b in defaults)
            {
                b.Id = await _brandRepo.AddAsync(b);
            }
            brands = defaults;
        }

        var dtos = brands.Select(b => new BrandDto
        {
            Id = b.Id,
            Name = b.Name
        });
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<BrandDto>> PostBrand([FromBody] BrandDto dto)
    {
        var entity = new Brand { Name = dto.Name };
        var newId = await _brandRepo.AddAsync(entity);
        return CreatedAtAction(nameof(GetBrands), null, new BrandDto { Id = newId, Name = entity.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBrand(string id, [FromBody] BrandDto dto)
    {
        var entity = await _brandRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        await _brandRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(string id)
    {
        await _brandRepo.DeleteAsync(id);
        return NoContent();
    }
}
