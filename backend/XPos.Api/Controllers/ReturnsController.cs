using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SaleReturnsController : ControllerBase
{
    private readonly ISaleReturnRepository _repository;
    private readonly IReturnService _returnService;

    public SaleReturnsController(ISaleReturnRepository repository, IReturnService returnService) 
    { 
        _repository = repository; 
        _returnService = returnService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _repository.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaleReturn saleReturn)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            saleReturn.UserId = userId;
            saleReturn.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(saleReturn.Ref))
        {
            saleReturn.Ref = $"SR-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _returnService.CreateSaleReturnAsync(saleReturn);
        return CreatedAtAction(nameof(GetById), new { id }, saleReturn);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, SaleReturn saleReturn)
    {
        if (id != saleReturn.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            saleReturn.UpdatedBy = userId;
        }

        return await _repository.UpdateAsync(saleReturn) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _repository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IPurchaseReturnRepository _repository;
    private readonly IReturnService _returnService;

    public PurchaseReturnsController(IPurchaseReturnRepository repository, IReturnService returnService) 
    { 
        _repository = repository; 
        _returnService = returnService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _repository.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PurchaseReturn purchaseReturn)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            purchaseReturn.UserId = userId;
            purchaseReturn.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(purchaseReturn.Ref))
        {
            purchaseReturn.Ref = $"PR-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _returnService.CreatePurchaseReturnAsync(purchaseReturn);
        return CreatedAtAction(nameof(GetById), new { id }, purchaseReturn);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, PurchaseReturn purchaseReturn)
    {
        if (id != purchaseReturn.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            purchaseReturn.UpdatedBy = userId;
        }

        return await _repository.UpdateAsync(purchaseReturn) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _repository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
