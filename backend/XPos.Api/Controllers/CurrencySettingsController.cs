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
    public async Task<IActionResult> Get() => Ok(await _currencyService.GetAsync());

    [HttpPost("currency")]
    public async Task<IActionResult> CreateOrUpdate(CurrencySetting currency)
    {
        var existing = await _currencyService.GetAsync();
        if (existing != null)
        {
            currency.Id = existing.Id;
            await _currencyService.UpdateAsync(currency);
        }
        else
        {
            var repo = GetCurrencyRepository();
            if (repo != null)
            {
                await repo.CreateAsync(currency);
            }
        }
        return Ok(new { id = currency.Id });
    }

    private ICurrencyRepository? GetCurrencyRepository()
    {
        var serviceProvider = HttpContext.RequestServices;
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetService(typeof(ICurrencyRepository)) as ICurrencyRepository;
    }
}
