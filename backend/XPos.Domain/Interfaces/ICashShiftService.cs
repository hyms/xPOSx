using System.Threading.Tasks;
using XPos.Domain.Dtos;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICashShiftService
{
    Task<long> OpenShiftAsync(long registerId, long userId, decimal startingCash, long activeWarehouseId);
    Task<string> ExecuteManualTransactionAsync(long shiftId, string type, decimal amount, string notes, string recipientName, long userId);
    Task<bool> CloseShiftAsync(long shiftId, decimal actualCash, string notes);
    Task<CashShiftReceiptDto?> GetReceiptPayloadAsync(long shiftId);
    Task<ActiveShiftDto?> GetActiveShiftAsync(long userId, long warehouseId);
}
