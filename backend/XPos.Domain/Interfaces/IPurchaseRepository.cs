using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IPurchaseRepository
{
    Task<PagedResult<PurchaseReadDto>> GetAllAsync(PagingParams pagingParams);
    Task<Purchase?> GetByIdAsync(long id);
    Task<long> CreateAsync(Purchase purchase);
    Task<bool> UpdateAsync(Purchase purchase);
    Task<bool> DeleteAsync(long id, long userId);
    Task UpdateVoucherIdAsync(long purchaseId, long voucherId);

}
