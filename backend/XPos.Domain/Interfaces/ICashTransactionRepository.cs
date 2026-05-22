using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICashTransactionRepository
{
    Task<CashTransaction?> GetByIdAsync(long id);
    Task<IEnumerable<CashTransaction>> GetByShiftIdAsync(long shiftId);
    Task<long> CreateAsync(CashTransaction transaction);
    Task<int> GetDailyTransactionCountAsync();
}
