using System.Threading.Tasks;
using XPos.Domain.Models;

using XPos.Domain.Dtos;

namespace XPos.Domain.Interfaces;

public interface IAdjustmentService
{
    Task<IEnumerable<AdjustmentReadDto>> GetAllAsync(string? filter = null);
    Task<Adjustment?> GetByIdAsync(long id);
    Task<long> CreateAdjustmentAsync(CreateAdjustmentDto dto, long userId);
    Task<bool> UpdateAdjustmentAsync(UpdateAdjustmentDto dto, long userId);
    Task<bool> DeleteAdjustmentAsync(long id, long userId);
}
