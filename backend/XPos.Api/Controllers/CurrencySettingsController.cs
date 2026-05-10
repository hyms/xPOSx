using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/settings")]
public class CurrencySettingsController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrencySettingsController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet("currency")]
    public async Task<IActionResult> Get() => Ok(await _currencyService.GetAllAsync());

    [HttpPost("currency")]
    public async Task<IActionResult> CreateOrUpdate(Currency currency)
    {
        var existing = (await _currencyService.GetAllAsync()).FirstOrDefault();
        if (existing != null)
        {
            currency.Id = existing.Id;
            await _currencyService.UpdateAsync(currency);
        }
        else
        {
            await _currencyService.CreateAsync(currency);
        }
        return Ok(new { id = currency.Id });
    }
}