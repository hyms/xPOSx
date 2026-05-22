using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Dtos;
using XPos.Domain.Interfaces;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CashShiftsController : ControllerBase
{
    private readonly ICashShiftService _cashShiftService;
    private readonly ICurrentUserService _currentUserService;

    public CashShiftsController(ICashShiftService cashShiftService, ICurrentUserService currentUserService)
    {
        _cashShiftService = cashShiftService;
        _currentUserService = currentUserService;
    }

    [HttpPost("open")]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftDto dto)
    {
        var userId = _currentUserService.UserId;
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;

        if (userId <= 0)
        {
            return Unauthorized(new { message = "Usuario no autenticado." });
        }
        if (!activeWarehouseId.HasValue)
        {
            return BadRequest(new { message = "Debe tener un almacén activo seleccionado." });
        }

        try
        {
            var shiftId = await _cashShiftService.OpenShiftAsync(dto.RegisterId, userId, dto.StartingCash, activeWarehouseId.Value);
            return Ok(new { shiftId, message = "Turno de caja abierto correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseShift([FromBody] CloseShiftDto dto)
    {
        try
        {
            var success = await _cashShiftService.CloseShiftAsync(dto.ShiftId, dto.ActualCash, dto.Notes ?? string.Empty);
            return success ? Ok(new { message = "Turno de caja cerrado correctamente." }) : BadRequest(new { message = "No se pudo cerrar el turno." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("transaction")]
    public async Task<IActionResult> ExecuteManualTransaction([FromBody] CashTransactionDto dto)
    {
        var userId = _currentUserService.UserId;
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Usuario no autenticado." });
        }

        try
        {
            var voucherNumber = await _cashShiftService.ExecuteManualTransactionAsync(
                dto.ShiftId, dto.TransactionType, dto.Amount, dto.Notes ?? string.Empty, dto.RecipientName ?? string.Empty, userId);
            return Ok(new { voucherNumber, message = "Transacción manual registrada correctamente." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveShift()
    {
        var userId = _currentUserService.UserId;
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;

        if (userId <= 0 || !activeWarehouseId.HasValue)
        {
            return Ok(null); // Return empty if no active shift can be queried
        }

        var activeShift = await _cashShiftService.GetActiveShiftAsync(userId, activeWarehouseId.Value);
        return Ok(activeShift);
    }

    [HttpGet("print/{shiftId}")]
    public async Task<IActionResult> GetReceiptPayload(long shiftId)
    {
        var payload = await _cashShiftService.GetReceiptPayloadAsync(shiftId);
        return payload == null ? NotFound() : Ok(payload);
    }
}
