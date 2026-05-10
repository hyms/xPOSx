using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IWarehouseRepository
{
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<Warehouse?> GetByIdAsync(long id);
    Task<long> CreateAsync(Warehouse warehouse);
    Task<bool> UpdateAsync(Warehouse warehouse);
    Task<bool> DeleteAsync(long id);
}
