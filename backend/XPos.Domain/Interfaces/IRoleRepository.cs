using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(long id);
    Task<long> CreateAsync(Role role);
    Task<bool> UpdateAsync(Role role);
    Task<bool> DeleteAsync(long id);
    Task<bool> AssignPermissionsAsync(long roleId, IEnumerable<long> permissionIds);
}
