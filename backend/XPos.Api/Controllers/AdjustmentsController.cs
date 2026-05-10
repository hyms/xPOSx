using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdjustmentsController : ControllerBase
{
    private readonly IAdjustmentRepository _adjustmentRepository;
    private readonly IAdjustmentService _adjustmentService;
    
    public AdjustmentsController(IAdjustmentRepository adjustmentRepository, IAdjustmentService adjustmentService) 
    { 
        _adjustmentRepository = adjustmentRepository; 
        _adjustmentService = adjustmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? filter = null) => Ok(await _adjustmentRepository.GetAllAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var adjustment = await _adjustmentRepository.GetByIdAsync(id);
        return adjustment == null ? NotFound() : Ok(adjustment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Adjustment adjustment)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            adjustment.UserId = userId;
            adjustment.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(adjustment.Ref))
        {
            adjustment.Ref = $"ADJ-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _adjustmentService.CreateAdjustmentAsync(adjustment);
        return CreatedAtAction(nameof(GetById), new { id }, adjustment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Adjustment adjustment)
    {
        if (id != adjustment.Id) return BadRequest();
        adjustment.UpdatedBy = long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
        return await _adjustmentRepository.UpdateAsync(adjustment) ? Ok(adjustment) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _adjustmentRepository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
