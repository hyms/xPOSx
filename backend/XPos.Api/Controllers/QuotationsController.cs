using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class QuotationsController : ControllerBase
{
    private readonly IQuotationRepository _repository;
    public QuotationsController(IQuotationRepository repository) { _repository = repository; }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _repository.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Quotation quotation)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            quotation.UserId = userId;
            quotation.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(quotation.Ref))
        {
            quotation.Ref = $"QUO-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _repository.CreateAsync(quotation);
        return CreatedAtAction(nameof(GetById), new { id }, quotation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Quotation quotation)
    {
        if (id != quotation.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            quotation.UpdatedBy = userId;
        }

        return await _repository.UpdateAsync(quotation) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _repository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
