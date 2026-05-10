using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize(Policy = "settings_view")]
[ApiController]
[Route("api/currencies")]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ICurrencyRepository _currencyRepository;

    public CurrenciesController(ICurrencyService currencyService, ICurrencyRepository currencyRepository)
    {
        _currencyService = currencyService;
        _currencyRepository = currencyRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _currencyRepository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var currency = await _currencyRepository.GetByIdAsync(id);
        return currency == null ? NotFound() : Ok(currency);
    }

    [Authorize(Policy = "settings_edit")]
    [HttpPost]
    public async Task<IActionResult> Create(CurrencySetting currency)
    {
        var id = await _currencyRepository.CreateAsync(currency);
        return CreatedAtAction(nameof(GetById), new { id }, currency);
    }

    [Authorize(Policy = "settings_edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, CurrencySetting currency)
    {
        if (id != currency.Id) return BadRequest();
        return await _currencyRepository.UpdateAsync(currency) ? NoContent() : NotFound();
    }

    [Authorize(Policy = "settings_delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        return await _currencyRepository.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
