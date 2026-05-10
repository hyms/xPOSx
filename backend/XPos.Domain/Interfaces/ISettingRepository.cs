using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ISettingRepository
{
    Task<Setting?> GetAsync();
    Task<bool> UpdateAsync(Setting setting);
}
