using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class CashTransactionRepository : ICashTransactionRepository
{
    private readonly IUnitOfWork _uow;

    public CashTransactionRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CashTransaction?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT id, cash_shift_id AS CashShiftId, voucher_number AS VoucherNumber, 
                   transaction_type AS TransactionType, amount, notes, 
                   recipient_name AS RecipientName, created_by AS CreatedBy, created_at AS CreatedAt 
            FROM cash_transactions 
            WHERE id = @id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<CashTransaction>(sql, new { id }, _uow.Transaction);
    }

    public async Task<IEnumerable<CashTransaction>> GetByShiftIdAsync(long shiftId)
    {
        const string sql = @"
            SELECT id, cash_shift_id AS CashShiftId, voucher_number AS VoucherNumber, 
                   transaction_type AS TransactionType, amount, notes, 
                   recipient_name AS RecipientName, created_by AS CreatedBy, created_at AS CreatedAt 
            FROM cash_transactions 
            WHERE cash_shift_id = @shiftId 
            ORDER BY created_at DESC";
        return await _uow.Connection.QueryAsync<CashTransaction>(sql, new { shiftId }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(CashTransaction transaction)
    {
        const string sql = @"
            INSERT INTO cash_transactions (cash_shift_id, voucher_number, transaction_type, amount, notes, recipient_name, created_by, created_at)
            VALUES (@CashShiftId, @VoucherNumber, @TransactionType, @Amount, @Notes, @RecipientName, @CreatedBy, @CreatedAt)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, transaction, _uow.Transaction);
    }

    public async Task<int> GetDailyTransactionCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM cash_transactions WHERE created_at::date = CURRENT_DATE";
        return await _uow.Connection.ExecuteScalarAsync<int>(sql, transaction: _uow.Transaction);
    }
}
