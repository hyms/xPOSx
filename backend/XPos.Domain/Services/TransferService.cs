using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

using XPos.Domain.Dtos;

namespace XPos.Domain.Services;

public class TransferService : ITransferService
{
    private readonly IUnitOfWork _uow;
    private readonly ITransferRepository _transferRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitRepository _unitRepository;
    private readonly UnitConversionService _unitConversionService;
    private readonly ICashShiftRepository _cashShiftRepository;

    public TransferService(
        IUnitOfWork uow,
        ITransferRepository transferRepository,
        IInventoryRepository inventoryRepository,
        IUnitRepository unitRepository,
        UnitConversionService unitConversionService,
        ICashShiftRepository cashShiftRepository)
    {
        _uow = uow;
        _transferRepository = transferRepository;
        _inventoryRepository = inventoryRepository;
        _unitRepository = unitRepository;
        _unitConversionService = unitConversionService;
        _cashShiftRepository = cashShiftRepository;
    }

    public async Task<IEnumerable<TransferReadDto>> GetAllAsync(string? filter = null)
    {
        return await _transferRepository.GetAllAsync(filter);
    }

    public async Task<Transfer?> GetByIdAsync(long id)
    {
        return await _transferRepository.GetByIdAsync(id);
    }

    public async Task<long> CreateTransferAsync(CreateTransferDto dto, long userId)
    {
        var transfer = new Transfer
        {
            Date = dto.Date,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            TaxRate = dto.TaxRate,
            Discount = dto.Discount,
            Shipping = dto.Shipping,
            Status = dto.Status,
            Notes = dto.Notes,
            UserId = userId,
            CreatedBy = userId,
            Ref = $"TR-{DateTime.Now:yyyyMMddHHmmss}",
            Details = dto.Details.Select(d => new TransferDetail
            {
                ProductId = d.ProductId,
                Cost = d.Cost,
                Quantity = d.Quantity,
                Total = d.Cost * d.Quantity
            }).ToList()
        };

        transfer.Items = transfer.Details.Count;
        transfer.GrandTotal = transfer.Details.Sum(d => d.Total) + transfer.Shipping;

        _uow.BeginTransaction();
        try
        {
            var transferId = await _transferRepository.CreateAsync(transfer);
            transfer.Id = transferId;

            foreach (var detail in transfer.Details)
            {
                // Subtract from origin
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.FromWarehouseId, -detail.Quantity);

                // Add to destination
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.ToWarehouseId, detail.Quantity);
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

    public async Task<bool> UpdateTransferAsync(UpdateTransferDto dto, long userId)
    {
        var existing = await _transferRepository.GetByIdAsync(dto.Id);
        if (existing == null) return false;

        var transfer = new Transfer
        {
            Id = dto.Id,
            Date = dto.Date,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            TaxRate = dto.TaxRate,
            Discount = dto.Discount,
            Shipping = dto.Shipping,
            Status = dto.Status,
            Notes = dto.Notes,
            UpdatedBy = userId,
            Details = dto.Details.Select(d => new TransferDetail
            {
                TransferId = dto.Id,
                ProductId = d.ProductId,
                Cost = d.Cost,
                Quantity = d.Quantity,
                Total = d.Cost * d.Quantity
            }).ToList()
        };

        transfer.Items = transfer.Details.Count;
        transfer.GrandTotal = transfer.Details.Sum(d => d.Total) + transfer.Shipping;

        _uow.BeginTransaction();
        try
        {
            // Reverse old stock
            foreach (var detail in existing.Details)
            {
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, existing.FromWarehouseId, detail.Quantity);
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, existing.ToWarehouseId, -detail.Quantity);
            }

            // Apply new stock
            foreach (var detail in transfer.Details)
            {
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.FromWarehouseId, -detail.Quantity);
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.ToWarehouseId, detail.Quantity);
            }

            var result = await _transferRepository.UpdateAsync(transfer);
            _uow.Commit();
            return result;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteTransferAsync(long id, long userId)
    {
        _uow.BeginTransaction();
        try
        {
            var transfer = await _transferRepository.GetByIdAsync(id);
            if (transfer == null)
            {
                _uow.Rollback();
                return false;
            }

            // Validar si la caja/turno del almacén de origen está cerrado para esta fecha
            var isFromClosed = await _cashShiftRepository.IsWarehouseClosedForDateAsync(transfer.FromWarehouseId, transfer.Date);
            if (isFromClosed)
            {
                throw new InvalidOperationException("No se puede eliminar la transferencia porque el turno de caja del almacén de origen ya está cerrado para esta fecha.");
            }

            // Validar si la caja/turno del almacén de destino está cerrado para esta fecha
            var isToClosed = await _cashShiftRepository.IsWarehouseClosedForDateAsync(transfer.ToWarehouseId, transfer.Date);
            if (isToClosed)
            {
                throw new InvalidOperationException("No se puede eliminar la transferencia porque el turno de caja del almacén de destino ya está cerrado para esta fecha.");
            }

            // 1. Revertir inventario (sumar en origen, restar en destino)
            if (transfer.Details != null)
            {
                foreach (var detail in transfer.Details)
                {
                    // Sumar de nuevo en origen
                    await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.FromWarehouseId, detail.Quantity);

                    // Restar de destino
                    await _inventoryRepository.UpdateStockAsync(detail.ProductId, transfer.ToWarehouseId, -detail.Quantity);
                }
            }

            // 2. Soft-delete de la transferencia
            var result = await _transferRepository.DeleteAsync(id, userId);

            _uow.Commit();
            return result;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
