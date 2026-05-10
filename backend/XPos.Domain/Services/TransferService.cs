using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class TransferService : ITransferService
{
    private readonly IUnitOfWork _uow;
    private readonly ITransferRepository _transferRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly UnitConversionService _unitConversionService;

    public TransferService(
        IUnitOfWork uow,
        ITransferRepository transferRepository,
        IInventoryRepository inventoryRepository,
        IUnitRepository unitRepository,
        UnitConversionService unitConversionService)
    {
        _uow = uow;
        _transferRepository = transferRepository;
        _inventoryRepository = inventoryRepository;
        _unitRepository = unitRepository;
        _unitConversionService = unitConversionService;
    }

    public async Task<long> CreateTransferAsync(Transfer transfer)
    {
        _uow.BeginTransaction();
        try
        {
            var transferId = await _transferRepository.CreateAsync(transfer);
            transfer.Id = transferId;

            foreach (var detail in transfer.Details)
            {
                Unit? unit = null;
                if (detail.PurchaseUnitId.HasValue)
                {
                    unit = await _unitRepository.GetByIdAsync(detail.PurchaseUnitId.Value);
                }

                var baseQuantity = _unitConversionService.CalculateBaseQuantity(detail.Quantity, unit);

                // Subtract from origin
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.FromWarehouseId, -baseQuantity);

                // Add to destination
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.ToWarehouseId, baseQuantity);
            }

            _uow.Commit();
            return transferId;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
