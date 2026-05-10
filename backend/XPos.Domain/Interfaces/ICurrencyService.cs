using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICurrencyService
{
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<Currency?> GetByIdAsync(long id);
    Task<long> CreateAsync(Currency currency);
    Task<bool> UpdateAsync(Currency currency);
    Task<bool> DeleteAsync(long id);
}
