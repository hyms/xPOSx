using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ISaleRepository
{
    Task<PagedResult<SaleReadDto>> GetAllAsync(PagingParams pagingParams);
    Task<Sale?> GetByIdAsync(long id);
    Task<long> CreateAsync(Sale sale);
    Task UpdateVoucherIdAsync(long saleId, long voucherId);
    Task<bool> DeleteAsync(long id, long userId);
}
