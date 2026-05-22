using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CashRegistersController : ControllerBase
{
    private readonly ICashRegisterRepository _cashRegisterRepository;

    public CashRegistersController(ICashRegisterRepository cashRegisterRepository)
    {
        _cashRegisterRepository = cashRegisterRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var registers = await _cashRegisterRepository.GetAllAsync();
        return Ok(registers);
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByWarehouse(long warehouseId)
    {
        var registers = await _cashRegisterRepository.GetByWarehouseIdAsync(warehouseId);
        return Ok(registers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var register = await _cashRegisterRepository.GetByIdAsync(id);
        return register == null ? NotFound() : Ok(register);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CashRegister register)
    {
        if (string.IsNullOrWhiteSpace(register.Name))
        {
            return BadRequest(new { message = "El nombre de la caja es requerido." });
        }
        var id = await _cashRegisterRepository.CreateAsync(register);
        register.Id = id;
        return CreatedAtAction(nameof(GetById), new { id }, register);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] CashRegister register)
    {
        if (id != register.Id)
        {
            return BadRequest(new { message = "ID mismatch." });
        }
        if (string.IsNullOrWhiteSpace(register.Name))
        {
            return BadRequest(new { message = "El nombre de la caja es requerido." });
        }
        var success = await _cashRegisterRepository.UpdateAsync(register);
        return success ? Ok(register) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var balance = await _cashRegisterRepository.GetBalanceAsync(id);
        if (balance != 0)
        {
            return BadRequest(new { message = $"No se puede eliminar la caja porque tiene un saldo de {balance:F2}. El saldo de la caja debe estar en 0.00 para poder eliminarla." });
        }
        var success = await _cashRegisterRepository.DeleteAsync(id);
        return success ? Ok(new { message = "Caja eliminada correctamente." }) : NotFound();
    }
}
