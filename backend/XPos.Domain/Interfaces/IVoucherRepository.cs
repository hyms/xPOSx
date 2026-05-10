using XPos.Domain.Models;
using System.Threading.Tasks;

namespace XPos.Domain.Interfaces;

public interface IVoucherRepository
{
    Task<Voucher?> GetByIdAsync(long id);
    Task<long> CreateAsync(Voucher voucher);
    Task<bool> UpdateAsync(Voucher voucher);
    Task<bool> DeleteAsync(long id);
}
