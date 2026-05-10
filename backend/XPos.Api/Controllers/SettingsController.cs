using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingRepository _settingRepository;

    public SettingsController(ISettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _settingRepository.GetAsync();
        if (settings == null) return NotFound();
        return Ok(settings);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Setting setting)
    {
        if (id != setting.Id) return BadRequest();
        var success = await _settingRepository.UpdateAsync(setting);
        if (!success) return NotFound();
        return NoContent();
    }
}
