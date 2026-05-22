using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/settings/currency")]
public class CurrencySettingsController : ControllerBase
{
    private readonly ISettingRepository _settingRepository;

    public CurrencySettingsController(ISettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _settingRepository.GetAsync();
        if (settings == null) return NotFound();

        var dto = new CurrencySettingDto
        {
            Id = settings.Id,
            Code = settings.CurrencyCode ?? "BOB",
            Symbol = settings.CurrencySymbol ?? "Bs"
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Update(CurrencySettingDto dto)
    {
        var settings = await _settingRepository.GetAsync();
        if (settings == null) return NotFound();

        settings.CurrencyCode = dto.Code;
        settings.CurrencySymbol = dto.Symbol;

        var success = await _settingRepository.UpdateAsync(settings);
        if (!success) return BadRequest("Error updating currency settings");

        return Ok(new { id = settings.Id });
    }
}

public class CurrencySettingDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}
