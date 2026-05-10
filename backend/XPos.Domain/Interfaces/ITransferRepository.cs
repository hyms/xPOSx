using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ITransferRepository
{
    Task<IEnumerable<TransferReadDto>> GetAllAsync(string? filter = null);
    Task<Transfer?> GetByIdAsync(long id);
    Task<long> CreateAsync(Transfer transfer);
    // Task<bool> UpdateAsync(Transfer transfer); // Often transfers are immutable or handled via status
    Task<bool> DeleteAsync(long id, long deletedBy);
}
