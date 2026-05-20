using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Text;

namespace XPos.Data.Repositories;

public class QuotationRepository : IQuotationRepository
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public QuotationRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<QuotationReadDto>> GetAllAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sql = new StringBuilder(@"
            SELECT q.id, q.ref, q.date::timestamp as Date, q.grand_total as GrandTotal, q.status,
                   c.name as ClientName, w.name as WarehouseName
            FROM quotations q
            JOIN clients c ON q.client_id = c.id
            JOIN warehouses w ON q.warehouse_id = w.id
            WHERE q.deleted_at IS NULL");

        var parameters = new DynamicParameters();

        if (!hasAllAccess)
        {
            sql.Append(" AND q.warehouse_id = @activeWarehouseId");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        sql.Append(" ORDER BY q.date DESC");
        
        return await _uow.Connection.QueryAsync<QuotationReadDto>(sql.ToString(), parameters, _uow.Transaction);
    }

    public async Task<Quotation?> GetByIdAsync(long id)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = "SELECT * FROM quotations WHERE id = @id AND deleted_at IS NULL";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        var quotation = await _uow.Connection.QueryFirstOrDefaultAsync<Quotation>(sql, parameters, _uow.Transaction);
        if (quotation != null)
        {
            const string detailsSql = "SELECT * FROM quotation_details WHERE quotation_id = @id";
            quotation.Details = (await _uow.Connection.QueryAsync<QuotationDetail>(detailsSql, new { id }, _uow.Transaction)).ToList();
        }
        return quotation;
    }

    public async Task<long> CreateAsync(Quotation quotation)
    {
        const string sql = @"
            INSERT INTO quotations (user_id, date, ref, client_id, warehouse_id, tax_rate, tax_net, discount, shipping, grand_total, status, notes, created_at, created_by)
            VALUES (@UserId, @Date, @Ref, @ClientId, @WarehouseId, @TaxRate, @TaxNet, @Discount, @Shipping, @GrandTotal, @Status, @Notes, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        
        var quotationId = await _uow.Connection.ExecuteScalarAsync<long>(sql, quotation, _uow.Transaction);

        foreach (var detail in quotation.Details)
        {
            detail.QuotationId = quotationId;
            const string detailSql = @"
                INSERT INTO quotation_details (quotation_id, product_id, product_variant_id, sale_unit_id, price, tax_net, tax_method, discount, discount_method, quantity, total, created_at)
                VALUES (@QuotationId, @ProductId, @ProductVariantId, @SaleUnitId, @Price, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, CURRENT_TIMESTAMP)";
            await _uow.Connection.ExecuteAsync(detailSql, detail, _uow.Transaction);
        }

        return quotationId;
    }

    public async Task<bool> UpdateAsync(Quotation quotation)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            UPDATE quotations SET
                date = @Date,
                ref = @Ref,
                client_id = @ClientId,
                warehouse_id = @WarehouseId,
                tax_rate = @TaxRate,
                tax_net = @TaxNet,
                discount = @Discount,
                shipping = @Shipping,
                grand_total = @GrandTotal,
                status = @Status,
                notes = @Notes,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @UpdatedBy
            WHERE id = @Id AND deleted_at IS NULL";
        
        var parameters = new DynamicParameters(quotation);

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

        string sql = "UPDATE quotations SET deleted_at = CURRENT_TIMESTAMP, updated_by = @userId WHERE id = @id AND deleted_at IS NULL";
        var parameters = new DynamicParameters(new { id, userId });

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sql, parameters, _uow.Transaction) > 0;
    }
}
