using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/settings")]
public class SmsSettingsApiController : ControllerBase
{
    private readonly ISmsSettingsService _settingsService;

    public SmsSettingsApiController(ISmsSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("sms")]
    public async Task<IActionResult> Get() => Ok(await _settingsService.GetAllAsync());

    [HttpPost("sms")]
    public async Task<IActionResult> CreateOrUpdate(SmsSettings settings)
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