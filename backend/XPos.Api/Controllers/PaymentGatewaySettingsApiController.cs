using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/settings")]
public class PaymentGatewaySettingsApiController : ControllerBase
{
    private readonly IPaymentGatewaySettingsService _settingsService;

    public PaymentGatewaySettingsApiController(IPaymentGatewaySettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("payment-gateway")]
    public async Task<IActionResult> Get() => Ok(await _settingsService.GetAllAsync());

    [HttpPost("payment-gateway")]
    public async Task<IActionResult> CreateOrUpdate(PaymentGatewaySettings settings)
    {
        var existing = (await _settingsService.GetAllAsync()).FirstOrDefault();
        if (existing != null)
        {
            settings.Id = existing.Id;
            await _settingsService.UpdateAsync(settings);
        }
        else
        {
            await _settingsService.CreateAsync(settings);
        }
        return Ok(new { id = settings.Id });
    }
}