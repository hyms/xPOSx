using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ISaleService
{
    Task<PagedResult<SaleReadDto>> GetAllAsync(PagingParams pagingParams);
    Task<Sale?> GetByIdAsync(long id);
    Task<long> CreateSaleAsync(Sale sale, long userId);
    Task<bool> DeleteSaleAsync(long id, long userId);
}
