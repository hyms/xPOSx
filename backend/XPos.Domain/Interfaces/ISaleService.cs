using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ISaleService
{
    Task<long> CreateSaleAsync(Sale sale);
}
