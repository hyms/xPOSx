using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Dtos;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SaleReturnsController : ControllerBase
{
    private readonly IReturnService _returnService;

    public SaleReturnsController(IReturnService returnService) 
    { 
        _returnService = returnService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _returnService.GetAllSaleReturnsAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _returnService.GetSaleReturnByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleReturnDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        var id = await _returnService.CreateSaleReturnAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _returnService.DeleteSaleReturnAsync(id, userId) ? NoContent() : NotFound();
    }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IReturnService _returnService;

    public PurchaseReturnsController(IReturnService returnService) 
    { 
        _returnService = returnService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _returnService.GetAllPurchaseReturnsAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _returnService.GetPurchaseReturnByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseReturnDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        var id = await _returnService.CreatePurchaseReturnAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _returnService.DeletePurchaseReturnAsync(id, userId) ? NoContent() : NotFound();
    }
}
