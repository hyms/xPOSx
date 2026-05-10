using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IPurchaseService
{
    Task<long> CreatePurchaseAsync(Purchase purchase);
}
