using System.Threading.Tasks;
using XPos.Domain.Models;

using XPos.Domain.Dtos;

namespace XPos.Domain.Interfaces;

public interface ITransferService
{
    Task<IEnumerable<TransferReadDto>> GetAllAsync(string? filter = null);
    Task<Transfer?> GetByIdAsync(long id);
    Task<long> CreateTransferAsync(CreateTransferDto dto, long userId);
    Task<bool> UpdateTransferAsync(UpdateTransferDto dto, long userId);
    Task<bool> DeleteTransferAsync(long id, long userId);
}
