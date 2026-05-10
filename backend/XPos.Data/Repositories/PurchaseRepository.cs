using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;
using System.Text;

namespace XPos.Data.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly IUnitOfWork _uow;

    public PurchaseRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<PurchaseReadDto>> GetAllAsync(PagingParams pagingParams)
    {
        var sqlBuilder = new StringBuilder(@"
            FROM purchases p
            JOIN providers pr ON p.provider_id = pr.id
            JOIN warehouses w ON p.warehouse_id = w.id
            WHERE p.deleted_at IS NULL
        ");

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(pagingParams.Filter))
        {
            sqlBuilder.Append(" AND (p.ref ILIKE @Filter OR pr.name ILIKE @Filter OR w.name ILIKE @Filter)");
            parameters.Add("Filter", $"%{pagingParams.Filter}%");
        }

        var countSql = "SELECT COUNT(*) " + sqlBuilder;
        var totalItems = await _uow.Connection.ExecuteScalarAsync<int>(countSql, parameters, _uow.Transaction);

        var sql = new StringBuilder("SELECT p.id, p.ref, p.date::timestamp as Date, p.grand_total as GrandTotal, p.paid_amount as PaidAmount, p.status, p.payment_status as PaymentStatus, pr.name as ProviderName, w.name as WarehouseName ").Append(sqlBuilder);

        if (!string.IsNullOrWhiteSpace(pagingParams.SortBy))
        {
            var sortOrder = pagingParams.SortDescending ? "DESC" : "ASC";
            // Basic sanitization to prevent SQL injection in column names
            var sortBySafe = new string(pagingParams.SortBy.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            if (!string.IsNullOrWhiteSpace(sortBySafe))
            {
                sql.Append($" ORDER BY p.{sortBySafe} {sortOrder}");
            }
        }
        else
        {
            sql.Append(" ORDER BY p.date DESC");
        }

        sql.Append(" LIMIT @PageSize OFFSET @Offset");
        parameters.Add("PageSize", pagingParams.PageSize);
        parameters.Add("Offset", (pagingParams.Page - 1) * pagingParams.PageSize);

        var items = await _uow.Connection.QueryAsync<PurchaseReadDto>(sql.ToString(), parameters, _uow.Transaction);

        return new PagedResult<PurchaseReadDto>
        {
            Items = items,
            TotalItems = totalItems,
            Page = pagingParams.Page,
            PageSize = pagingParams.PageSize
        };
    }

    public async Task<Purchase?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT p.id, p.user_id as UserId, p.ref, p.date::timestamp as Date, p.provider_id as ProviderId, p.warehouse_id as WarehouseId, 
                   p.tax_rate as TaxRate, p.tax_net as TaxNet, p.discount, p.shipping, p.grand_total as GrandTotal, 
                   p.paid_amount as PaidAmount, p.status, p.payment_status as PaymentStatus, p.notes, 
                   p.created_at as CreatedAt, p.updated_at as UpdatedAt, p.deleted_at as DeletedAt, 
                   p.created_by as CreatedBy, p.updated_by as UpdatedBy, p.deleted_by as DeletedBy,
                   p.voucher_id as VoucherId
            FROM purchases p
            WHERE p.id = @id AND p.deleted_at IS NULL";
        
        var purchase = await _uow.Connection.QueryFirstOrDefaultAsync<Purchase>(sql, new { id }, _uow.Transaction);

        if (purchase != null)
        {
            const string detailsSql = @"
                SELECT pd.*, prod.name as ProductName 
                FROM purchase_details pd
                JOIN products prod ON pd.product_id = prod.id
                WHERE pd.purchase_id = @id";
            purchase.Details = (await _uow.Connection.QueryAsync<PurchaseDetail>(detailsSql, new { id }, _uow.Transaction)).ToList();
        }
        return purchase;
    }

    public async Task<long> CreateAsync(Purchase purchase)
    {
        const string sql = @"
            INSERT INTO purchases (user_id, ref, date, provider_id, warehouse_id, tax_rate, tax_net, discount, shipping, grand_total, paid_amount, status, payment_status, notes, created_at, created_by)
            VALUES (@UserId, @Ref, @Date, @ProviderId, @WarehouseId, @TaxRate, @TaxNet, @Discount, @Shipping, @GrandTotal, @PaidAmount, @Status, @PaymentStatus, @Notes, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        
        var purchaseId = await _uow.Connection.ExecuteScalarAsync<long>(sql, purchase, _uow.Transaction);

        foreach (var detail in purchase.Details)
        {
            detail.PurchaseId = purchaseId;
            const string detailSql = @"
                INSERT INTO purchase_details (purchase_id, product_id, product_variant_id, purchase_unit_id, cost, tax_net, tax_method, discount, discount_method, quantity, total, created_at, created_by)
                VALUES (@PurchaseId, @ProductId, @ProductVariantId, @PurchaseUnitId, @Cost, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, CURRENT_TIMESTAMP, @CreatedBy)";
            await _uow.Connection.ExecuteAsync(detailSql, new { 
                detail.PurchaseId, detail.ProductId, detail.ProductVariantId, detail.PurchaseUnitId, 
                detail.Cost, detail.TaxNet, detail.TaxMethod, detail.Discount, detail.DiscountMethod, 
                detail.Quantity, detail.Total, CreatedBy = purchase.CreatedBy 
            }, _uow.Transaction);
        }

        return purchaseId;
    }

    public async Task<bool> UpdateAsync(Purchase purchase)
    {
        const string sql = @"
            UPDATE purchases SET 
                provider_id = @ProviderId,
                warehouse_id = @WarehouseId,
                grand_total = @GrandTotal,
                paid_amount = @PaidAmount,
                status = @Status, 
                payment_status = @PaymentStatus, 
                notes = @Notes, 
                updated_at = CURRENT_TIMESTAMP, 
                updated_by = @UpdatedBy 
            WHERE id = @Id";

        var affected = await _uow.Connection.ExecuteAsync(sql, purchase, _uow.Transaction);
        Console.WriteLine($"Affected rows: {affected}, PurchaseId: {purchase.Id}, GrandTotal: {purchase.GrandTotal}");

        if (affected > 0 && purchase.Details != null)
        {
            await _uow.Connection.ExecuteAsync(
                "DELETE FROM purchase_details WHERE purchase_id = @Id",
                new { purchase.Id }, _uow.Transaction);

            foreach (var detail in purchase.Details)
            {
                detail.PurchaseId = purchase.Id;
                const string detailSql = @"
                    INSERT INTO purchase_details (purchase_id, product_id, product_variant_id, purchase_unit_id, cost, tax_net, tax_method, discount, discount_method, quantity, total, created_at, created_by)
                    VALUES (@PurchaseId, @ProductId, @ProductVariantId, @PurchaseUnitId, @Cost, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, CURRENT_TIMESTAMP, @CreatedBy)";
                await _uow.Connection.ExecuteAsync(detailSql, new {
                    detail.PurchaseId, detail.ProductId, detail.ProductVariantId, detail.PurchaseUnitId,
                    detail.Cost, detail.TaxNet, detail.TaxMethod, detail.Discount, detail.DiscountMethod,
                    detail.Quantity, detail.Total, CreatedBy = purchase.UpdatedBy
                }, _uow.Transaction);
            }
        }

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(long id, long deletedBy)
    {
        const string sql = "UPDATE purchases SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @deletedBy WHERE id = @id";
        return await _uow.Connection.ExecuteAsync(sql, new { id, deletedBy }, _uow.Transaction) > 0;
    }

    public async Task UpdateVoucherIdAsync(long purchaseId, long voucherId)
    {
        const string updatePurchaseSql = "UPDATE purchases SET voucher_id = @VoucherId WHERE id = @Id";
        await _uow.Connection.ExecuteAsync(updatePurchaseSql, new { VoucherId = voucherId, Id = purchaseId }, _uow.Transaction);
    }
}
