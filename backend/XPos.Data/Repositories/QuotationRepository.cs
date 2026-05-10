using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;

namespace XPos.Data.Repositories;

public class QuotationRepository : IQuotationRepository
{
    private readonly string _connectionString;
    public QuotationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();
    }
    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<QuotationReadDto>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT q.id, q.ref, q.date, q.grand_total as GrandTotal, q.status,
                   c.name as ClientName, w.name as WarehouseName
            FROM quotations q
            JOIN clients c ON q.client_id = c.id
            JOIN warehouses w ON q.warehouse_id = w.id
            WHERE q.deleted_at IS NULL
            ORDER BY q.date DESC";
        return await connection.QueryAsync<QuotationReadDto>(sql);
    }

    public async Task<Quotation?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM quotations WHERE id = @id AND deleted_at IS NULL";
        var quotation = await connection.QueryFirstOrDefaultAsync<Quotation>(sql, new { id });
        if (quotation != null)
        {
            const string detailsSql = "SELECT * FROM quotation_details WHERE quotation_id = @id";
            quotation.Details = (await connection.QueryAsync<QuotationDetail>(detailsSql, new { id })).ToList();
        }
        return quotation;
    }

    public async Task<long> CreateAsync(Quotation quotation)
    {
        using var connection = (NpgsqlConnection)CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            const string sql = @"
                INSERT INTO quotations (user_id, date, ref, client_id, warehouse_id, tax_rate, tax_net, discount, shipping, grand_total, status, notes, created_at, created_by)
                VALUES (@UserId, @Date, @Ref, @ClientId, @WarehouseId, @TaxRate, @TaxNet, @Discount, @Shipping, @GrandTotal, @Status, @Notes, CURRENT_TIMESTAMP, @CreatedBy)
                RETURNING id";
            
            var quotationId = await connection.ExecuteScalarAsync<long>(sql, quotation, transaction);

            foreach (var detail in quotation.Details)
            {
                detail.QuotationId = quotationId;
                const string detailSql = @"
                    INSERT INTO quotation_details (quotation_id, product_id, product_variant_id, sale_unit_id, price, tax_net, tax_method, discount, discount_method, quantity, total, created_at)
                    VALUES (@QuotationId, @ProductId, @ProductVariantId, @SaleUnitId, @Price, @TaxNet, @TaxMethod, @Discount, @DiscountMethod, @Quantity, @Total, CURRENT_TIMESTAMP)";
                await connection.ExecuteAsync(detailSql, detail, transaction);
            }

            await transaction.CommitAsync();
            return quotationId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Quotation quotation)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE quotations SET 
                status = @Status, 
                notes = @Notes, 
                updated_at = CURRENT_TIMESTAMP, 
                updated_by = @UpdatedBy 
            WHERE id = @Id";
        return await connection.ExecuteAsync(sql, quotation) > 0;
    }

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE quotations SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @userId WHERE id = @id";
        return await connection.ExecuteAsync(sql, new { id, userId }) > 0;
    }
}
