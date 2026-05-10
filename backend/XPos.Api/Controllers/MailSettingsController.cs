using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize(Policy = "settings_edit")] // Edit permission for mail settings
[ApiController]
[Route("api/[controller]")]
public class MailSettingsController : ControllerBase
{
    private readonly IMailSettingsService _mailSettingsService;

    public MailSettingsController(IMailSettingsService mailSettingsService)
    {
        _mailSettingsService = mailSettingsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _mailSettingsService.GetAsync());

    [HttpPost("create-or-update")]
    public async Task<IActionResult> CreateOrUpdate(MailSettings mailSettings)
    {
        var id = await _mailSettingsService.CreateOrUpdateAsync(mailSettings);
        return Ok(new { id });
    }
}
