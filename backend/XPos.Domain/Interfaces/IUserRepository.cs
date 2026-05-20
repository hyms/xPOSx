using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<long> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> UpdatePasswordAsync(long id, string newPassword);
    Task<bool> DeleteAsync(long id);
    Task<bool> ToggleUserStatusAsync(long id);
    Task<IEnumerable<long>> GetUserWarehouseIdsAsync(long userId);
}
