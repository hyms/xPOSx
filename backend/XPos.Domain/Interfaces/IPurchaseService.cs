using System.Threading.Tasks;
using XPos.Domain.Models;

using XPos.Domain.Dtos;

namespace XPos.Domain.Interfaces;

public interface IPurchaseService
{
    Task<PagedResult<PurchaseReadDto>> GetAllAsync(PagingParams pagingParams);
    Task<Purchase?> GetByIdAsync(long id);
    Task<long> CreatePurchaseAsync(CreatePurchaseDto dto, long userId);
    Task<bool> UpdatePurchaseAsync(UpdatePurchaseDto dto, long userId);
    Task<bool> DeletePurchaseAsync(long id, long userId);
}
