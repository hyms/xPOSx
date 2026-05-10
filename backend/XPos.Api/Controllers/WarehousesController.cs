using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehousesController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var warehouses = await _warehouseRepository.GetAllAsync();
        return Ok(warehouses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse == null) return NotFound();
        return Ok(warehouse);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Warehouse warehouse)
    {
        var id = await _warehouseRepository.CreateAsync(warehouse);
        warehouse.Id = id;
        return CreatedAtAction(nameof(GetById), new { id }, warehouse);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Warehouse warehouse)
    {
        if (id != warehouse.Id) return BadRequest();
        var success = await _warehouseRepository.UpdateAsync(warehouse);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await _warehouseRepository.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
