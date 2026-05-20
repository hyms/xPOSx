using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;
using System.Text;

namespace XPos.Data.Repositories;

public class SaleReturnRepository : ISaleReturnRepository
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public SaleReturnRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<SaleReturnReadDto>> GetAllAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder(@"
            SELECT sr.id, sr.ref, sr.date::timestamp as Date, sr.grand_total as GrandTotal, sr.status, sr.payment_status as PaymentStatus, 
                   c.name as ClientName, w.name as WarehouseName
            FROM sale_returns sr
            JOIN clients c ON sr.client_id = c.id
            JOIN warehouses w ON sr.warehouse_id = w.id
            WHERE sr.deleted_at IS NULL
            ");
        var parameters = new DynamicParameters();

        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND sr.warehouse_id = @activeWarehouseId");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        sqlBuilder.Append(" ORDER BY sr.date DESC");

        return await _uow.Connection.QueryAsync<SaleReturnReadDto>(sqlBuilder.ToString(), parameters, _uow.Transaction);
    }

    public async Task<SaleReturn?> GetByIdAsync(long id)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            SELECT sr.*, v.id as VoucherId, v.* 
            FROM sale_returns sr 
            LEFT JOIN vouchers v ON sr.voucher_id = v.id 
            WHERE sr.id = @id AND sr.deleted_at IS NULL";
        
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        if (!hasAllAccess)
        {
            sql += " AND sr.warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        var saleReturns = await _uow.Connection.QueryAsync<SaleReturn, Voucher, SaleReturn>(
            sql, 
            (sr, v) => { sr.Voucher = v; return sr; }, 
            parameters, 
            splitOn: "VoucherId", 
            transaction: _uow.Transaction);
            
        var saleReturn = saleReturns.FirstOrDefault();

        if (saleReturn != null)
        {
            const string detailsSql = @"
                SELECT srd.*, p.name as ProductName 
                FROM sale_return_details srd
                JOIN products p ON srd.product_id = p.id
                WHERE srd.sale_return_id = @id";
            saleReturn.Details = (await _uow.Connection.QueryAsync<SaleReturnDetail>(detailsSql, new { id }, _uow.Transaction)).ToList();
        }
        return saleReturn;
    }

    public async Task<long> CreateAsync(SaleReturn saleReturn)
    {
        const string sql = @"
            INSERT INTO sale_returns (user_id, date, ref, sale_id, client_id, warehouse_id, tax_rate, tax_net, discount, shipping, grand_total, paid_amount, status, payment_status, notes, voucher_id, created_at, created_by)
            VALUES (@UserId, @Date, @Ref, @SaleId, @ClientId, @WarehouseId, @TaxRate, @TaxNet, @Discount, @Shipping, @GrandTotal, @PaidAmount, @Status, @PaymentStatus, @Notes, @VoucherId, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        
        var returnId = await _uow.Connection.ExecuteScalarAsync<long>(sql, saleReturn, _uow.Transaction);

        foreach (var detail in saleReturn.Details)
        {
            detail.SaleReturnId = returnId;
            const string detailSql = @"
                INSERT INTO sale_return_details (sale_return_id, product_id, product_variant_id, sale_unit_id, price, tax_net, tax_method, discount, discount_method, quantity, total, created_at)
                VALUES (@SaleReturnId, @ProductId, @ProductVariantId, @SaleUnitId, @Price, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, CURRENT_TIMESTAMP)";
            await _uow.Connection.ExecuteAsync(detailSql, detail, _uow.Transaction);
        }

        return returnId;
    }

    public async Task<bool> UpdateAsync(SaleReturn saleReturn)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            UPDATE sale_returns SET 
                status = @Status, 
                payment_status = @PaymentStatus, 
                notes = @Notes, 
                updated_at = CURRENT_TIMESTAMP, 
                updated_by = @UpdatedBy 
            WHERE id = @Id AND deleted_at IS NULL";
        
        var parameters = new DynamicParameters(saleReturn);

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sql, parameters, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = "UPDATE sale_returns SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @userId WHERE id = @id AND deleted_at IS NULL";
        var parameters = new DynamicParameters(new { id, userId });

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sql, parameters, _uow.Transaction) > 0;
    }

    public async Task UpdateVoucherIdAsync(long saleReturnId, long voucherId)
    {
        const string updateSaleReturnSql = "UPDATE sale_returns SET voucher_id = @VoucherId WHERE id = @Id";
        await _uow.Connection.ExecuteAsync(updateSaleReturnSql, new { VoucherId = voucherId, Id = saleReturnId }, _uow.Transaction);
    }
}

public class PurchaseReturnRepository : IPurchaseReturnRepository
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public PurchaseReturnRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<PurchaseReturnReadDto>> GetAllAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder(@"
            SELECT pr.id, pr.ref, pr.date::timestamp as Date, pr.grand_total as GrandTotal, pr.status, pr.payment_status as PaymentStatus, 
                   p.name as ProviderName, w.name as WarehouseName
            FROM purchase_returns pr
            JOIN providers p ON pr.provider_id = p.id
            JOIN warehouses w ON pr.warehouse_id = w.id
            WHERE pr.deleted_at IS NULL
            ");
        var parameters = new DynamicParameters();

        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND pr.warehouse_id = @activeWarehouseId");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        sqlBuilder.Append(" ORDER BY pr.date DESC");

        return await _uow.Connection.QueryAsync<PurchaseReturnReadDto>(sqlBuilder.ToString(), parameters, _uow.Transaction);
    }

    public async Task<PurchaseReturn?> GetByIdAsync(long id)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            SELECT pr.*, v.id as VoucherId, v.* 
            FROM purchase_returns pr 
            LEFT JOIN vouchers v ON pr.voucher_id = v.id 
            WHERE pr.id = @id AND pr.deleted_at IS NULL";
        
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        if (!hasAllAccess)
        {
            sql += " AND pr.warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        var purchaseReturns = await _uow.Connection.QueryAsync<PurchaseReturn, Voucher, PurchaseReturn>(
            sql, 
            (pr, v) => { pr.Voucher = v; return pr; }, 
            parameters, 
            splitOn: "VoucherId", 
            transaction: _uow.Transaction);
            
        var purchaseReturn = purchaseReturns.FirstOrDefault();

        if (purchaseReturn != null)
        {
            const string detailsSql = @"
                SELECT prd.*, p.name as ProductName 
                FROM purchase_return_details prd
                JOIN products p ON prd.product_id = p.id
                WHERE prd.purchase_return_id = @id";
            purchaseReturn.Details = (await _uow.Connection.QueryAsync<PurchaseReturnDetail>(detailsSql, new { id }, _uow.Transaction)).ToList();
        }
        return purchaseReturn;
    }

    public async Task<long> CreateAsync(PurchaseReturn purchaseReturn)
    {
        const string sql = @"
            INSERT INTO purchase_returns (user_id, date, ref, purchase_id, provider_id, warehouse_id, tax_rate, tax_net, discount, shipping, grand_total, paid_amount, status, payment_status, notes, voucher_id, created_at, created_by)
            VALUES (@UserId, @Date, @Ref, @PurchaseId, @ProviderId, @WarehouseId, @TaxRate, @TaxNet, @Discount, @Shipping, @GrandTotal, @PaidAmount, @Status, @PaymentStatus, @Notes, @VoucherId, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        
        var returnId = await _uow.Connection.ExecuteScalarAsync<long>(sql, purchaseReturn, _uow.Transaction);

        foreach (var detail in purchaseReturn.Details)
        {
            detail.PurchaseReturnId = returnId;
            const string detailSql = @"
                INSERT INTO purchase_return_details (purchase_return_id, product_id, product_variant_id, purchase_unit_id, cost, tax_net, tax_method, discount, discount_method, quantity, total, created_at)
                VALUES (@PurchaseReturnId, @ProductId, @ProductVariantId, @PurchaseUnitId, @Cost, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, CURRENT_TIMESTAMP)";
            await _uow.Connection.ExecuteAsync(detailSql, detail, _uow.Transaction);
        }

        return returnId;
    }

    public async Task<bool> UpdateAsync(PurchaseReturn purchaseReturn)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            UPDATE purchase_returns SET 
                status = @Status, 
                payment_status = @PaymentStatus, 
                notes = @Notes, 
                updated_at = CURRENT_TIMESTAMP, 
                updated_by = @UpdatedBy 
            WHERE id = @Id AND deleted_at IS NULL";
        
        var parameters = new DynamicParameters(purchaseReturn);

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sql, parameters, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = "UPDATE purchase_returns SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @userId WHERE id = @id AND deleted_at IS NULL";
        var parameters = new DynamicParameters(new { id, userId });

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sql, parameters, _uow.Transaction) > 0;
    }

    public async Task UpdateVoucherIdAsync(long purchaseReturnId, long voucherId)
    {
        const string updatePurchaseReturnSql = "UPDATE purchase_returns SET voucher_id = @VoucherId WHERE id = @Id";
        await _uow.Connection.ExecuteAsync(updatePurchaseReturnSql, new { VoucherId = voucherId, Id = purchaseReturnId }, _uow.Transaction);
    }
}
