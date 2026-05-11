using System.Threading.Tasks;
using XPos.Domain.Models;

using XPos.Domain.Dtos;

namespace XPos.Domain.Interfaces;

public interface IReturnService
{
    Task<IEnumerable<SaleReturnReadDto>> GetAllSaleReturnsAsync();
    Task<SaleReturn?> GetSaleReturnByIdAsync(long id);
    Task<long> CreateSaleReturnAsync(CreateSaleReturnDto dto, long userId);
    Task<bool> DeleteSaleReturnAsync(long id, long userId);

    Task<IEnumerable<PurchaseReturnReadDto>> GetAllPurchaseReturnsAsync();
    Task<PurchaseReturn?> GetPurchaseReturnByIdAsync(long id);
    Task<long> CreatePurchaseReturnAsync(CreatePurchaseReturnDto dto, long userId);
    Task<bool> DeletePurchaseReturnAsync(long id, long userId);
}
