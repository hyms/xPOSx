using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICurrencyRepository
{
    Task<CurrencySetting?> GetAsync();
    Task<CurrencySetting?> GetByIdAsync(long id);
    Task<List<CurrencySetting>> GetAllAsync();
    Task<long> CreateAsync(CurrencySetting currency);
    Task<bool> UpdateAsync(CurrencySetting currency);
    Task<bool> DeleteAsync(long id);
}
