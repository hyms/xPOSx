using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

using XPos.Domain.Dtos;

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

    public async Task<IEnumerable<AdjustmentReadDto>> GetAllAsync(string? filter = null)
    {
        return await _adjustmentRepository.GetAllAsync(filter);
    }

    public async Task<Adjustment?> GetByIdAsync(long id)
    {
        return await _adjustmentRepository.GetByIdAsync(id);
    }

    public async Task<long> CreateAdjustmentAsync(CreateAdjustmentDto dto, long userId)
    {
        var adjustment = new Adjustment
        {
            Date = dto.Date,
            WarehouseId = dto.WarehouseId,
            Notes = dto.Notes,
            UserId = userId,
            CreatedBy = userId,
            Ref = $"ADJ-{DateTime.Now:yyyyMMddHHmmss}",
            Items = dto.Details.Count,
            Details = dto.Details.Select(d => new AdjustmentDetail
            {
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                Type = d.Type,
                CreatedBy = userId
            }).ToList()
        };

        _uow.BeginTransaction();
        try
        {
            var adjustmentId = await _adjustmentRepository.CreateAsync(adjustment);
            adjustment.Id = adjustmentId;

            foreach (var detail in adjustment.Details)
            {
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

    public async Task<bool> UpdateAdjustmentAsync(UpdateAdjustmentDto dto, long userId)
    {
        var existing = await _adjustmentRepository.GetByIdAsync(dto.Id);
        if (existing == null) return false;

        var adjustment = new Adjustment
        {
            Id = dto.Id,
            Date = dto.Date,
            WarehouseId = dto.WarehouseId,
            Notes = dto.Notes,
            UpdatedBy = userId,
            Items = dto.Details.Count,
            Details = dto.Details.Select(d => new AdjustmentDetail
            {
                AdjustmentId = dto.Id,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                Type = d.Type,
                CreatedBy = userId // For simplicity in the repository update logic
            }).ToList()
        };

        _uow.BeginTransaction();
        try
        {
            // Reverse old stock
            foreach (var detail in existing.Details)
            {
                double reverseQuantity = detail.Type == "add" ? -detail.Quantity : detail.Quantity;
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, existing.WarehouseId, reverseQuantity);
            }

            // Apply new stock
            foreach (var detail in adjustment.Details)
            {
                double quantityChange = detail.Type == "add" ? detail.Quantity : -detail.Quantity;
                await _inventoryRepository.UpdateStockAsync(detail.ProductId, adjustment.WarehouseId, quantityChange);
            }

            var result = await _adjustmentRepository.UpdateAsync(adjustment);
            _uow.Commit();
            return result;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAdjustmentAsync(long id, long userId)
    {
        return await _adjustmentRepository.DeleteAsync(id, userId);
    }
}
