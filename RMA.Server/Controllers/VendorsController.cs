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
public class VendorsController : ControllerBase
{
    private readonly FirestoreRepository<Vendor> _vendorRepo;

    public VendorsController(FirestoreRepository<Vendor> vendorRepo)
    {
        _vendorRepo = vendorRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetVendors()
    {
        var vendors = await _vendorRepo.GetAllAsync();
        
        // Auto seed
        if (!vendors.Any())
        {
            var defaults = new List<Vendor>
            {
                new() { Name = "Kết Nối Xanh", ContactPerson = "Anh Hùng", Phone = "0901234567" },
                new() { Name = "Nguyễn Kim", ContactPerson = "Chị Vy", Phone = "18006800" },
                new() { Name = "HP Service", WarrantyLink = "https://support.hp.com" },
                new() { Name = "FPT Services" }
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
            Name = v.Name,
            ContactPerson = v.ContactPerson,
            Phone = v.Phone,
            Email = v.Email,
            Address = v.Address,
            WarrantyLink = v.WarrantyLink,
            Note = v.Note
        });
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<VendorDto>> PostVendor([FromBody] VendorDto dto)
    {
        var entity = new Vendor 
        { 
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            WarrantyLink = dto.WarrantyLink,
            Note = dto.Note
        };
        var newId = await _vendorRepo.AddAsync(entity);
        return CreatedAtAction(nameof(GetVendors), null, new VendorDto { Id = newId, Name = entity.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutVendor(string id, [FromBody] VendorDto dto)
    {
        var entity = await _vendorRepo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        
        entity.Name = dto.Name;
        entity.ContactPerson = dto.ContactPerson;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Address = dto.Address;
        entity.WarrantyLink = dto.WarrantyLink;
        entity.Note = dto.Note;
        
        await _vendorRepo.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(string id)
    {
        await _vendorRepo.DeleteAsync(id);
        return NoContent();
    }
}
