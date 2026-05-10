using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IPurchaseService _purchaseService;
    
    public PurchasesController(IPurchaseRepository purchaseRepository, IPurchaseService purchaseService) 
    { 
        _purchaseRepository = purchaseRepository; 
        _purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagingParams pagingParams) => Ok(await _purchaseRepository.GetAllAsync(pagingParams));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(id);
        return purchase == null ? NotFound() : Ok(purchase);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Purchase purchase)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            purchase.UserId = userId;
            purchase.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(purchase.Ref))
        {
            purchase.Ref = $"PR-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _purchaseService.CreatePurchaseAsync(purchase);
        return CreatedAtAction(nameof(GetById), new { id }, purchase);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Purchase purchase)
    {
        if (id != purchase.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            purchase.UpdatedBy = userId;
        }

        return await _purchaseRepository.UpdateAsync(purchase) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _purchaseRepository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
