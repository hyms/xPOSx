using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICurrencyService
{
    Task<CurrencySetting?> GetAsync();
    Task<bool> UpdateAsync(CurrencySetting currency);
}
