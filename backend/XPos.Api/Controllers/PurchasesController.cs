using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Dtos;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    
    public PurchasesController(IPurchaseService purchaseService) 
    { 
        _purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagingParams pagingParams) => Ok(await _purchaseService.GetAllAsync(pagingParams));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var purchase = await _purchaseService.GetByIdAsync(id);
        return purchase == null ? NotFound() : Ok(purchase);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        var id = await _purchaseService.CreatePurchaseAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, UpdatePurchaseDto dto)
    {
        if (id != dto.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        return await _purchaseService.UpdatePurchaseAsync(dto, userId) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _purchaseService.DeletePurchaseAsync(id, userId) ? NoContent() : NotFound();
    }
}
