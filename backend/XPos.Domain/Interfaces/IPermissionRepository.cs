using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(long id);
    Task<IEnumerable<Permission>> GetByRoleIdAsync(long roleId);
}
