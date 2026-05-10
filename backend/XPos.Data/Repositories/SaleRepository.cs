using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;

namespace XPos.Data.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly IUnitOfWork _uow;

    public SaleRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<SaleReadDto>> GetAllAsync(PagingParams pagingParams)
    {
        var validSortColumns = new Dictionary<string, string>
        {
            ["date"] = "s.date",
            ["ref"] = "s.ref",
            ["clientName"] = "c.name",
            ["warehouseName"] = "w.name",
            ["grandTotal"] = "s.grand_total",
            ["paidAmount"] = "s.paid_amount",
            ["status"] = "s.status",
            ["paymentStatus"] = "s.payment_status"
        };

        var sortBy = validSortColumns.ContainsKey(pagingParams.SortBy ?? "date") 
            ? validSortColumns[pagingParams.SortBy ?? "date"] 
            : "s.date";

        var sortOrder = pagingParams.SortDescending ? "DESC" : "ASC";

        var baseSql = @"
            FROM sales s
            JOIN clients c ON s.client_id = c.id
            JOIN warehouses w ON s.warehouse_id = w.id
            WHERE s.deleted_at IS NULL
            ";

        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(pagingParams.Filter))
        {
            whereClauses.Add("(s.ref ILIKE @Filter OR c.name ILIKE @Filter OR w.name ILIKE @Filter)");
            parameters.Add("Filter", $"%{pagingParams.Filter}%");
        }

        if (whereClauses.Any())
        {
            baseSql += " AND " + string.Join(" AND ", whereClauses);
        }

        var countSql = "SELECT COUNT(s.id)" + baseSql;
        var total = await _uow.Connection.ExecuteScalarAsync<int>(countSql, parameters, _uow.Transaction);

        var dataSql = $@"
             SELECT s.id, s.ref, s.date, s.grand_total as GrandTotal, s.paid_amount as PaidAmount, s.status, s.payment_status as PaymentStatus, s.voucher_id as VoucherId,
                    c.name as ClientName, w.name as WarehouseName
            {baseSql}
            ORDER BY {sortBy} {sortOrder}
            LIMIT @PageSize OFFSET @Offset
            ";
        
        parameters.Add("PageSize", pagingParams.PageSize);
        parameters.Add("Offset", (pagingParams.Page - 1) * pagingParams.PageSize);

        var items = await _uow.Connection.QueryAsync<SaleReadDto>(dataSql, parameters, _uow.Transaction);

        return new PagedResult<SaleReadDto> { 
            Items = items, 
            TotalItems = total, 
            Page = pagingParams.Page,
            PageSize = pagingParams.PageSize
        };
    }

    public async Task<Sale?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT s.*, v.id as VoucherId, v.* 
            FROM sales s 
            LEFT JOIN vouchers v ON s.voucher_id = v.id 
            WHERE s.id = @id AND s.deleted_at IS NULL";
        
        var sales = await _uow.Connection.QueryAsync<Sale, Voucher, Sale>(
            sql, 
            (s, v) => { s.Voucher = v; return s; }, 
            new { id }, 
            splitOn: "VoucherId", 
            transaction: _uow.Transaction);
            
        var sale = sales.FirstOrDefault();

        if (sale != null)
        {
            const string detailsSql = @"
                SELECT sd.*, p.name as ProductName 
                FROM sale_details sd
                JOIN products p ON sd.product_id = p.id
                WHERE sd.sale_id = @id";
            sale.Details = (await _uow.Connection.QueryAsync<SaleDetail>(detailsSql, new { id }, _uow.Transaction)).ToList();
        }
        return sale;
    }

    public async Task<long> CreateAsync(Sale sale)
    {
        const string sql = @"
            INSERT INTO sales (user_id, ref, date, is_pos, client_id, warehouse_id, tax_rate, tax_net, discount, shipping, grand_total, paid_amount, status, payment_status, shipping_status, notes, created_at, created_by)
            VALUES (@UserId, @Ref, @Date, @IsPos, @ClientId, @WarehouseId, @TaxRate, @TaxNet, @Discount, @Shipping, @GrandTotal, @PaidAmount, @Status, @PaymentStatus, @ShippingStatus, @Notes, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        
        var saleId = await _uow.Connection.ExecuteScalarAsync<long>(sql, sale, _uow.Transaction);

        foreach (var detail in sale.Details)
        {
            detail.SaleId = saleId;
            const string detailSql = @"
                INSERT INTO sale_details (sale_id, product_id, product_variant_id, sale_unit_id, price, tax_net, tax_method, discount, discount_method, quantity, total, date, created_at, created_by)
                VALUES (@SaleId, @ProductId, @ProductVariantId, @SaleUnitId, @Price, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, @Date, CURRENT_TIMESTAMP, @CreatedBy)";
            await _uow.Connection.ExecuteAsync(detailSql, new { 
                detail.SaleId, detail.ProductId, detail.ProductVariantId, detail.SaleUnitId, 
                detail.Price, detail.TaxNet, detail.TaxMethod, detail.Discount, detail.DiscountMethod, 
                detail.Quantity, detail.Total, Date = sale.Date, CreatedBy = sale.CreatedBy 
            }, _uow.Transaction);
        }

        return saleId;
    }

    public async Task UpdateVoucherIdAsync(long saleId, long voucherId)
    {
        const string updateSaleSql = "UPDATE sales SET voucher_id = @VoucherId WHERE id = @Id";
        await _uow.Connection.ExecuteAsync(updateSaleSql, new { VoucherId = voucherId, Id = saleId }, _uow.Transaction);
    }

    public async Task<bool> DeleteAsync(long id, long deletedBy)
    {
        const string sql = "UPDATE sales SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @deletedBy WHERE id = @id";
        return await _uow.Connection.ExecuteAsync(sql, new { id, deletedBy }, _uow.Transaction) > 0;
    }
}
