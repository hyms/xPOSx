using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ISmsSettingsRepository
{
    Task<IEnumerable<SmsSettings>> GetAllAsync();
    Task<SmsSettings?> GetByIdAsync(long id);
    Task<long> CreateAsync(SmsSettings settings);
    Task<bool> UpdateAsync(SmsSettings settings);
    Task<bool> DeleteAsync(long id);
}
