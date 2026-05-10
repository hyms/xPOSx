using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize(Policy = "settings_view")] // Assuming a general setting view permission
[ApiController]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _currencyService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var currency = await _currencyService.GetByIdAsync(id);
        return currency == null ? NotFound() : Ok(currency);
    }

    [Authorize(Policy = "settings_edit")]
    [HttpPost]
    public async Task<IActionResult> Create(Currency currency)
    {
        var id = await _currencyService.CreateAsync(currency);
        return CreatedAtAction(nameof(GetById), new { id }, currency);
    }

    [Authorize(Policy = "settings_edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Currency currency)
    {
        if (id != currency.Id) return BadRequest();
        return await _currencyService.UpdateAsync(currency) ? NoContent() : NotFound();
    }

    [Authorize(Policy = "settings_delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        return await _currencyService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
