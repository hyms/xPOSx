using System.Threading.Tasks;

namespace XPos.Domain.Interfaces;

public interface IInventoryRepository
{
    Task UpdateStockAsync(long productId, long warehouseId, decimal quantityChange);
}
