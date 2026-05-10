using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IReturnService
{
    Task<long> CreateSaleReturnAsync(SaleReturn saleReturn);
    Task<long> CreatePurchaseReturnAsync(PurchaseReturn purchaseReturn);
}
