using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly ITransferRepository _transferRepository;
    private readonly ITransferService _transferService;
    
    public TransfersController(ITransferRepository transferRepository, ITransferService transferService) 
    { 
        _transferRepository = transferRepository; 
        _transferService = transferService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? filter = null) => Ok(await _transferRepository.GetAllAsync(filter));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var transfer = await _transferRepository.GetByIdAsync(id);
        return transfer == null ? NotFound() : Ok(transfer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Transfer transfer)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            transfer.UserId = userId;
            transfer.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(transfer.Ref))
        {
            transfer.Ref = $"TR-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _transferService.CreateTransferAsync(transfer);
        return CreatedAtAction(nameof(GetById), new { id }, transfer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _transferRepository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
