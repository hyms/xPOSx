using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Dtos;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;
    
    public TransfersController(ITransferService transferService) 
    { 
        _transferService = transferService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? filter = null) => Ok(await _transferService.GetAllAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var transfer = await _transferService.GetByIdAsync(id);
        return transfer == null ? NotFound() : Ok(transfer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransferDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        var id = await _transferService.CreateTransferAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, UpdateTransferDto dto)
    {
        if (id != dto.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        return await _transferService.UpdateTransferAsync(dto, userId) ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _transferService.DeleteTransferAsync(id, userId) ? NoContent() : NotFound();
    }
}
