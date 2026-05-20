using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ISettingRepository
{
    Task<Setting?> GetAsync();
    Task<bool> UpdateAsync(Setting setting);
    Task<bool> UpdateMediaAsync(string type, string path);
}
