using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IAdjustmentRepository
{
    Task<IEnumerable<AdjustmentReadDto>> GetAllAsync(string? filter = null);
    Task<Adjustment?> GetByIdAsync(long id);
    Task<long> CreateAsync(Adjustment adjustment);
    Task<bool> DeleteAsync(long id, long userId);
}
