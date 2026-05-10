using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/currencies")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyRepository _currencyRepository;

    public CurrencyController(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var currency = await _currencyRepository.GetAsync();
        if (currency == null)
        {
            // If not found, return a default currency setting
            return Ok(new CurrencySetting { Code = "USD", Symbol = "$" });
        }
        return Ok(currency);
    }

    [HttpPost]
    public async Task<IActionResult> Update(CurrencySetting currency)
    {
        var success = await _currencyRepository.UpdateAsync(currency);
        if (!success)
        {
            return NotFound();
        }
        return Ok(currency);
    }
}
