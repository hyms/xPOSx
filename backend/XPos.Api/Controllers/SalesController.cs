using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleService _saleService;
    
    public SalesController(ISaleRepository saleRepository, ISaleService saleService) 
    { 
        _saleRepository = saleRepository; 
        _saleService = saleService;
    }

    [Authorize(Policy = "sales_view")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagingParams pagingParams) => Ok(await _saleRepository.GetAllAsync(pagingParams));

    [Authorize(Policy = "sales_view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var sale = await _saleRepository.GetByIdAsync(id);
        return sale == null ? NotFound() : Ok(sale);
    }

    [Authorize(Policy = "sales_create")]
    [HttpPost]
    public async Task<IActionResult> Create(Sale sale)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            sale.UserId = userId;
            sale.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(sale.Ref))
        {
            sale.Ref = $"SL-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _saleService.CreateSaleAsync(sale);
        return CreatedAtAction(nameof(GetById), new { id }, sale);
    }

    [Authorize(Policy = "sales_delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _saleRepository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
