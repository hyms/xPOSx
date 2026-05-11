using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Dtos;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdjustmentsController : ControllerBase
{
    private readonly IAdjustmentService _adjustmentService;
    
    public AdjustmentsController(IAdjustmentService adjustmentService) 
    { 
        _adjustmentService = adjustmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? filter = null) => Ok(await _adjustmentService.GetAllAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var adjustment = await _adjustmentService.GetByIdAsync(id);
        return adjustment == null ? NotFound() : Ok(adjustment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAdjustmentDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        var id = await _adjustmentService.CreateAdjustmentAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, UpdateAdjustmentDto dto)
    {
        if (id != dto.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);

        return await _adjustmentService.UpdateAdjustmentAsync(dto, userId) ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _adjustmentService.DeleteAdjustmentAsync(id, userId) ? NoContent() : NotFound();
    }
}
