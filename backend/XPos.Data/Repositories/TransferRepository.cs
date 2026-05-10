using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;

namespace XPos.Data.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly IUnitOfWork _uow;

    public TransferRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<TransferReadDto>> GetAllAsync(string? filter = null)
    {
        var sql = @"
            SELECT t.id, t.ref, t.date, t.items, t.grand_total as GrandTotal, t.status,
                   fw.name as FromWarehouseName, tw.name as ToWarehouseName
            FROM transfers t
            JOIN warehouses fw ON t.from_warehouse_id = fw.id
            JOIN warehouses tw ON t.to_warehouse_id = tw.id
            WHERE t.deleted_at IS NULL
            ";

        if (!string.IsNullOrWhiteSpace(filter))
        {
            sql += " AND (t.ref ILIKE @filter OR fw.name ILIKE @filter OR tw.name ILIKE @filter)";
        }
        
        sql += " ORDER BY t.date DESC";

        return await _uow.Connection.QueryAsync<TransferReadDto>(sql, new { filter = $"%{filter}%" }, _uow.Transaction);
    }

    public async Task<Transfer?> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM transfers WHERE id = @id AND deleted_at IS NULL";
        var transfer = await _uow.Connection.QueryFirstOrDefaultAsync<Transfer>(sql, new { id }, _uow.Transaction);
        if (transfer != null)
        {
            const string detailsSql = "SELECT * FROM transfer_details WHERE transfer_id = @id";
            transfer.Details = (await _uow.Connection.QueryAsync<TransferDetail>(detailsSql, new { id }, _uow.Transaction)).ToList();
        }
        return transfer;
    }

    public async Task<long> CreateAsync(Transfer transfer)
    {
        const string sql = @"
            INSERT INTO transfers (user_id, ref, date, from_warehouse_id, to_warehouse_id, items, tax_rate, tax_net, discount, shipping, grand_total, status, notes, created_at, created_by)
            VALUES (@UserId, @Ref, @Date, @FromWarehouseId, @ToWarehouseId, @Items, @TaxRate, @TaxNet, @Discount, @Shipping, @GrandTotal, @Status, @Notes, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        
        var transferId = await _uow.Connection.ExecuteScalarAsync<long>(sql, transfer, _uow.Transaction);

        foreach (var detail in transfer.Details)
        {
            detail.TransferId = transferId;
            const string detailSql = @"
                INSERT INTO transfer_details (transfer_id, product_id, product_variant_id, purchase_unit_id, cost, tax_net, tax_method, discount, discount_method, quantity, total, created_at, created_by)
                VALUES (@TransferId, @ProductId, @ProductVariantId, @PurchaseUnitId, @Cost, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, CURRENT_TIMESTAMP, @CreatedBy)";
            await _uow.Connection.ExecuteAsync(detailSql, new { 
                detail.TransferId, detail.ProductId, detail.ProductVariantId, detail.PurchaseUnitId, 
                detail.Cost, detail.TaxNet, detail.TaxMethod, detail.Discount, detail.DiscountMethod, 
                detail.Quantity, detail.Total, CreatedBy = transfer.CreatedBy 
            }, _uow.Transaction);
        }

        return transferId;
    }

    public async Task<bool> DeleteAsync(long id, long deletedBy)
    {
        const string sql = "UPDATE transfers SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @deletedBy WHERE id = @id";
        return await _uow.Connection.ExecuteAsync(sql, new { id, deletedBy }, _uow.Transaction) > 0;
    }
}
