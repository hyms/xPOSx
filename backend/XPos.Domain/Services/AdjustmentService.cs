using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class AdjustmentService : IAdjustmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IAdjustmentRepository _adjustmentRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public AdjustmentService(
        IUnitOfWork uow,
        IAdjustmentRepository adjustmentRepository,
        IInventoryRepository inventoryRepository)
    {
        _uow = uow;
        _adjustmentRepository = adjustmentRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<long> CreateAdjustmentAsync(Adjustment adjustment)
    {
        _uow.BeginTransaction();
        try
        {
            var adjustmentId = await _adjustmentRepository.CreateAsync(adjustment);
            adjustment.Id = adjustmentId;

            foreach (var detail in adjustment.Details)
            {
                // Adjustments directly affect stock based on type
                double quantityChange = detail.Type == "add" ? detail.Quantity : -detail.Quantity;
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, adjustment.WarehouseId, quantityChange);
            }

            _uow.Commit();
            return adjustmentId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
