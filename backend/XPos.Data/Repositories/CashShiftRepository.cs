using System.Threading.Tasks;
using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class CashShiftRepository : ICashShiftRepository
{
    private readonly IUnitOfWork _uow;

    public CashShiftRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CashShift?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT id, cash_register_id AS CashRegisterId, user_id AS UserId, status, 
                   opened_at AS OpenedAt, closed_at AS ClosedAt, starting_cash AS StartingCash, 
                   ending_cash_expected AS EndingCashExpected, ending_cash_actual AS EndingCashActual, 
                   discrepancy AS Discrepancy, closing_notes AS ClosingNotes 
            FROM cash_shifts 
            WHERE id = @id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<CashShift>(sql, new { id }, _uow.Transaction);
    }

    public async Task<CashShift?> GetActiveShiftAsync(long userId, long warehouseId)
    {
        const string sql = @"
            SELECT cs.id, cs.cash_register_id AS CashRegisterId, cs.user_id AS UserId, cs.status, 
                   cs.opened_at AS OpenedAt, cs.closed_at AS ClosedAt, cs.starting_cash AS StartingCash, 
                   cs.ending_cash_expected AS EndingCashExpected, cs.ending_cash_actual AS EndingCashActual, 
                   cs.discrepancy AS Discrepancy, cs.closing_notes AS ClosingNotes 
            FROM cash_shifts cs
            JOIN cash_registers cr ON cs.cash_register_id = cr.id
            WHERE cs.user_id = @userId AND cs.status = 'OPEN' AND cr.warehouse_id = @warehouseId
            LIMIT 1";
        return await _uow.Connection.QueryFirstOrDefaultAsync<CashShift>(sql, new { userId, warehouseId }, _uow.Transaction);
    }

    public async Task<CashShift?> GetActiveShiftByRegisterIdAsync(long registerId)
    {
        const string sql = @"
            SELECT id, cash_register_id AS CashRegisterId, user_id AS UserId, status, 
                   opened_at AS OpenedAt, closed_at AS ClosedAt, starting_cash AS StartingCash, 
                   ending_cash_expected AS EndingCashExpected, ending_cash_actual AS EndingCashActual, 
                   discrepancy AS Discrepancy, closing_notes AS ClosingNotes 
            FROM cash_shifts 
            WHERE cash_register_id = @registerId AND status = 'OPEN'
            LIMIT 1";
        return await _uow.Connection.QueryFirstOrDefaultAsync<CashShift>(sql, new { registerId }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(CashShift shift)
    {
        const string sql = @"
            INSERT INTO cash_shifts (cash_register_id, user_id, status, opened_at, starting_cash, ending_cash_expected)
            VALUES (@CashRegisterId, @UserId, @Status, @OpenedAt, @StartingCash, @EndingCashExpected)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, shift, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(CashShift shift)
    {
        const string sql = @"
            UPDATE cash_shifts
            SET status = @Status, 
                closed_at = @ClosedAt, 
                ending_cash_expected = @EndingCashExpected, 
                ending_cash_actual = @EndingCashActual, 
                discrepancy = @Discrepancy, 
                closing_notes = @ClosingNotes
            WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, shift, _uow.Transaction) > 0;
    }

    public async Task<decimal> GetCashSalesAmountAsync(long shiftId)
    {
        const string sql = @"
            SELECT COALESCE(SUM(ps.amount), 0)
            FROM payment_sales ps
            JOIN sales s ON ps.sale_id = s.id
            WHERE s.cash_shift_id = @shiftId AND ps.reglement = 'Cash' AND s.deleted_at IS NULL";
        return await _uow.Connection.ExecuteScalarAsync<decimal>(sql, new { shiftId }, _uow.Transaction);
    }

    public async Task<decimal> GetManualTransactionsSumAsync(long shiftId, string type)
    {
        const string sql = @"
            SELECT COALESCE(SUM(amount), 0)
            FROM cash_transactions
            WHERE cash_shift_id = @shiftId AND transaction_type = @type";
        return await _uow.Connection.ExecuteScalarAsync<decimal>(sql, new { shiftId, type }, _uow.Transaction);
    }

    public async Task<bool> IsWarehouseClosedForDateAsync(long warehouseId, DateTime date)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM cash_shifts cs
            JOIN cash_registers cr ON cs.cash_register_id = cr.id
            WHERE cr.warehouse_id = @warehouseId 
              AND cs.status = 'CLOSED' 
              AND cs.opened_at::date = @date::date";
        var count = await _uow.Connection.ExecuteScalarAsync<int>(sql, new { warehouseId, date = date.Date }, _uow.Transaction);
        return count > 0;
    }
}
