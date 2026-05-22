using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICashShiftRepository
{
    Task<CashShift?> GetByIdAsync(long id);
    Task<CashShift?> GetActiveShiftAsync(long userId, long warehouseId);
    Task<CashShift?> GetActiveShiftByRegisterIdAsync(long registerId);
    Task<long> CreateAsync(CashShift shift);
    Task<bool> UpdateAsync(CashShift shift);
    Task<decimal> GetCashSalesAmountAsync(long shiftId);
    Task<decimal> GetManualTransactionsSumAsync(long shiftId, string type);
    Task<bool> IsWarehouseClosedForDateAsync(long warehouseId, DateTime date);
}
