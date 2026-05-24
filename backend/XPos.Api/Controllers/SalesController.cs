using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Api.Filters;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    
    public SalesController(ISaleService saleService) 
    { 
        _saleService = saleService;
    }

    [Authorize(Policy = "sales_view")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagingParams pagingParams) => Ok(await _saleService.GetAllAsync(pagingParams));

    [Authorize(Policy = "sales_view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var sale = await _saleService.GetByIdAsync(id);
        return sale == null ? NotFound() : Ok(sale);
    }

    [Authorize(Policy = "sales_create")]
    [ServiceFilter(typeof(RequireOpenShiftFilter))]
    [HttpPost]
    public async Task<IActionResult> Create(Sale sale)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        if (HttpContext.Items.TryGetValue("ActiveCashShiftId", out var shiftIdObj) && shiftIdObj is long shiftId)
        {
            sale.CashShiftId = shiftId;
        }

        var id = await _saleService.CreateSaleAsync(sale, userId);
        return CreatedAtAction(nameof(GetById), new { id }, sale);
    }

    [Authorize(Policy = "sales_create")]
    [ServiceFilter(typeof(RequireOpenShiftFilter))]
    [HttpPatch("{id}/verify")]
    public async Task<IActionResult> Verify(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        if (!HttpContext.Items.TryGetValue("ActiveCashShiftId", out var shiftIdObj) || shiftIdObj is not long shiftId)
        {
            return StatusCode(403, new { message = "Debes abrir un turno de caja antes de verificar ventas." });
        }

        try
        {
            var success = await _saleService.VerifyOnlineSaleAsync(id, userId, shiftId);
            if (!success)
            {
                return NotFound(new { message = "Venta online no encontrada o no válida." });
            }

            var updatedSale = await _saleService.GetByIdAsync(id);
            return Ok(new 
            { 
                message = "Venta online verificada, aprobada, facturada por el SIAT y stock descontado exitosamente.",
                sale = updatedSale
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error al verificar la venta online: {ex.Message}" });
        }
    }

    [Authorize(Policy = "sales_delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _saleService.DeleteSaleAsync(id, userId) ? NoContent() : NotFound();
    }
}
