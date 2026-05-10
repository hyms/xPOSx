using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;

namespace XPos.Data.Repositories;

public class AdjustmentRepository : IAdjustmentRepository
{
    private readonly IUnitOfWork _uow;

    public AdjustmentRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<AdjustmentReadDto>> GetAllAsync(string? filter = null)
    {
        var sql = @"
            SELECT a.id, a.ref, a.date, a.items, w.name as WarehouseName
            FROM adjustments a
            JOIN warehouses w ON a.warehouse_id = w.id
            WHERE a.deleted_at IS NULL
            ";
            
        if (!string.IsNullOrWhiteSpace(filter))
        {
            sql += " AND (a.ref ILIKE @filter OR w.name ILIKE @filter)";
        }
        
        sql += " ORDER BY a.date DESC";

        return await _uow.Connection.QueryAsync<AdjustmentReadDto>(sql, new { filter = $"%{filter}%" }, _uow.Transaction);
    }

    public async Task<Adjustment?> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM adjustments WHERE id = @id AND deleted_at IS NULL";
        var adjustment = await _uow.Connection.QueryFirstOrDefaultAsync<Adjustment>(sql, new { id }, _uow.Transaction);
        if (adjustment != null)
        {
            const string detailsSql = "SELECT * FROM adjustment_details WHERE adjustment_id = @id";
            adjustment.Details = (await _uow.Connection.QueryAsync<AdjustmentDetail>(detailsSql, new { id }, _uow.Transaction)).ToList();
        }
        return adjustment;
    }

    public async Task<long> CreateAsync(Adjustment adjustment)
    {
        const string sql = @"
            INSERT INTO adjustments (user_id, ref, date, warehouse_id, items, notes, created_at, created_by)
            VALUES (@UserId, @Ref, @Date, @WarehouseId, @Items, @Notes, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        
        var adjustmentId = await _uow.Connection.ExecuteScalarAsync<long>(sql, adjustment, _uow.Transaction);

        foreach (var detail in adjustment.Details)
        {
            detail.AdjustmentId = adjustmentId;
            const string detailSql = @"
                INSERT INTO adjustment_details (adjustment_id, product_id, product_variant_id, quantity, type, created_at, created_by)
                VALUES (@AdjustmentId, @ProductId, @ProductVariantId, @Quantity, @Type, CURRENT_TIMESTAMP, @CreatedBy)";
            await _uow.Connection.ExecuteAsync(detailSql, new { 
                detail.AdjustmentId, detail.ProductId, detail.ProductVariantId, 
                detail.Quantity, detail.Type, CreatedBy = adjustment.CreatedBy 
            }, _uow.Transaction);
        }

        return adjustmentId;
    }

    public async Task<bool> DeleteAsync(long id, long deletedBy)
    {
        const string sql = "UPDATE adjustments SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @deletedBy WHERE id = @id";
        return await _uow.Connection.ExecuteAsync(sql, new { id, deletedBy }, _uow.Transaction) > 0;
    }
}
