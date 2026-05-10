using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize(Policy = "settings_view")] 
[ApiController]
[Route("api/[controller]")]
public class PaymentGatewaySettingsController : ControllerBase
{
    private readonly IPaymentGatewaySettingsService _settingsService;

    public PaymentGatewaySettingsController(IPaymentGatewaySettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _settingsService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var settings = await _settingsService.GetByIdAsync(id);
        return settings == null ? NotFound() : Ok(settings);
    }

    [Authorize(Policy = "settings_edit")]
    [HttpPost]
    public async Task<IActionResult> Create(PaymentGatewaySettings settings)
    {
        var id = await _settingsService.CreateAsync(settings);
        return CreatedAtAction(nameof(GetById), new { id }, settings);
    }

    [Authorize(Policy = "settings_edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, PaymentGatewaySettings settings)
    {
        if (id != settings.Id) return BadRequest();
        return await _settingsService.UpdateAsync(settings) ? NoContent() : NotFound();
    }

    [Authorize(Policy = "settings_delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        return await _settingsService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
