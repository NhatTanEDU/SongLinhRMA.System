using Microsoft.AspNetCore.Mvc;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMA.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly FirestoreRepository<Customer> _repository;

    public CustomersController(FirestoreRepository<Customer> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> Get()
    {
        var customers = await _repository.GetAllAsync();
        var dtos = customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            ContactPerson = c.ContactPerson,
            Phone = c.Phone,
            Email = c.Email,
            Address = c.Address,
            AvatarUrl = c.AvatarUrl,
            CreatedAt = c.CreatedAt
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> Get(string id)
    {
        var c = await _repository.GetByIdAsync(id);
        if (c == null) return NotFound();
        return new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            ContactPerson = c.ContactPerson,
            Phone = c.Phone,
            Email = c.Email,
            Address = c.Address,
            AvatarUrl = c.AvatarUrl,
            CreatedAt = c.CreatedAt
        };
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Post([FromBody] CustomerCreateDto dto)
    {
        var entity = new Customer
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            CreatedAt = DateTime.UtcNow
        };
        var newId = await _repository.AddAsync(entity);
        entity.Id = newId;

        return CreatedAtAction(nameof(Get), new { id = newId }, new CustomerDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ContactPerson = entity.ContactPerson,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            CreatedAt = entity.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] CustomerCreateDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.Name = dto.Name;
        entity.ContactPerson = dto.ContactPerson;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Address = dto.Address;

        await _repository.UpdateAsync(id, entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
