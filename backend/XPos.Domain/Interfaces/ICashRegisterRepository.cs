using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICashRegisterRepository
{
    Task<CashRegister?> GetByIdAsync(long id);
    Task<IEnumerable<CashRegister>> GetByWarehouseIdAsync(long warehouseId);
    Task<IEnumerable<CashRegister>> GetAllAsync();
    Task<long> CreateAsync(CashRegister register);
    Task<bool> UpdateAsync(CashRegister register);
    Task<bool> DeleteAsync(long id);
    Task<decimal> GetBalanceAsync(long registerId);
}
