using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class VoucherRepository : IVoucherRepository
{
    private readonly IUnitOfWork _uow;

    public VoucherRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Voucher?> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM vouchers WHERE id = @Id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<Voucher>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Voucher voucher)
    {
        // For legal invoices (SIN Bolivia), we must ensure gapless numbering within the same transaction
        if (voucher.VoucherType.Contains("Factura", StringComparison.OrdinalIgnoreCase))
        {
            // Lock the settings table (or a dedicated sequences table) to get the next number
            const string lockSql = "SELECT invoice_number FROM settings FOR UPDATE";
            var lastNumber = await _uow.Connection.ExecuteScalarAsync<int>(lockSql, null, _uow.Transaction);
            
            var nextNumber = lastNumber + 1;
            voucher.VoucherNumber = nextNumber.ToString();
            
            const string updateSettingsSql = "UPDATE settings SET invoice_number = @NextNumber";
            await _uow.Connection.ExecuteAsync(updateSettingsSql, new { NextNumber = nextNumber }, _uow.Transaction);
        }

        const string sql = @"
            INSERT INTO vouchers (sale_id, purchase_id, sale_return_id, purchase_return_id, voucher_type, voucher_number, cae, cae_expiration, issued_at, created_at)
            VALUES (@SaleId, @PurchaseId, @SaleReturnId, @PurchaseReturnId, @VoucherType, @VoucherNumber, @Cae, @CaeExpiration, @IssuedAt, CURRENT_TIMESTAMP)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, voucher, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Voucher voucher)
    {
        const string sql = @"
            UPDATE vouchers SET
                sale_id = @SaleId,
                purchase_id = @PurchaseId,
                sale_return_id = @SaleReturnId,
                purchase_return_id = @PurchaseReturnId,
                voucher_type = @VoucherType,
                voucher_number = @VoucherNumber,
                cae = @Cae,
                cae_expiration = @CaeExpiration,
                issued_at = @IssuedAt,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        voucher.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, voucher, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM vouchers WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction) > 0;
    }
}
