using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;

    public CurrencyService(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<CurrencySetting?> GetAsync()
    {
        return await _currencyRepository.GetAsync();
    }

    public async Task<bool> UpdateAsync(CurrencySetting currency)
    {
        return await _currencyRepository.UpdateAsync(currency);
    }
}
