using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ISaleReturnRepository
{
    Task<IEnumerable<SaleReturnReadDto>> GetAllAsync();
    Task<SaleReturn?> GetByIdAsync(long id);
    Task<long> CreateAsync(SaleReturn saleReturn);
    Task<bool> UpdateAsync(SaleReturn saleReturn);
    Task<bool> DeleteAsync(long id, long userId);
    Task UpdateVoucherIdAsync(long saleReturnId, long voucherId);
}

public interface IPurchaseReturnRepository
{
    Task<IEnumerable<PurchaseReturnReadDto>> GetAllAsync();
    Task<PurchaseReturn?> GetByIdAsync(long id);
    Task<long> CreateAsync(PurchaseReturn purchaseReturn);
    Task<bool> UpdateAsync(PurchaseReturn purchaseReturn);
    Task<bool> DeleteAsync(long id, long userId);
    Task UpdateVoucherIdAsync(long purchaseReturnId, long voucherId);
}
