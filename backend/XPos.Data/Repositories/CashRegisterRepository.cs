using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class CashRegisterRepository : ICashRegisterRepository
{
    private readonly IUnitOfWork _uow;

    public CashRegisterRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CashRegister?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, name, warehouse_id AS WarehouseId, is_active AS IsActive, is_matriz AS IsMatriz, created_at AS CreatedAt FROM cash_registers WHERE id = @id AND deleted_at IS NULL";
        return await _uow.Connection.QueryFirstOrDefaultAsync<CashRegister>(sql, new { id }, _uow.Transaction);
    }

    public async Task<IEnumerable<CashRegister>> GetByWarehouseIdAsync(long warehouseId)
    {
        const string sql = "SELECT id, name, warehouse_id AS WarehouseId, is_active AS IsActive, is_matriz AS IsMatriz, created_at AS CreatedAt FROM cash_registers WHERE warehouse_id = @warehouseId AND deleted_at IS NULL ORDER BY name";
        return await _uow.Connection.QueryAsync<CashRegister>(sql, new { warehouseId }, _uow.Transaction);
    }

    public async Task<IEnumerable<CashRegister>> GetAllAsync()
    {
        const string sql = "SELECT id, name, warehouse_id AS WarehouseId, is_active AS IsActive, is_matriz AS IsMatriz, created_at AS CreatedAt FROM cash_registers WHERE deleted_at IS NULL ORDER BY name";
        return await _uow.Connection.QueryAsync<CashRegister>(sql, transaction: _uow.Transaction);
    }

    public async Task<long> CreateAsync(CashRegister register)
    {
        const string sql = @"
            INSERT INTO cash_registers (name, warehouse_id, is_active, is_matriz, created_at)
            VALUES (@Name, @WarehouseId, @IsActive, @IsMatriz, CURRENT_TIMESTAMP)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, register, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(CashRegister register)
    {
        const string sql = @"
            UPDATE cash_registers
            SET name = @Name, warehouse_id = @WarehouseId, is_active = @IsActive, is_matriz = @IsMatriz, updated_at = CURRENT_TIMESTAMP
            WHERE id = @Id AND deleted_at IS NULL";
        return await _uow.Connection.ExecuteAsync(sql, register, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "UPDATE cash_registers SET deleted_at = CURRENT_TIMESTAMP WHERE id = @id AND deleted_at IS NULL";
        return await _uow.Connection.ExecuteAsync(sql, new { id }, _uow.Transaction) > 0;
    }

    public async Task<decimal> GetBalanceAsync(long registerId)
    {
        const string sql = @"
            SELECT COALESCE(SUM(balance), 0) FROM (
                -- Closed shifts balance (actual closing cash remaining)
                SELECT COALESCE(ending_cash_actual, 0) AS balance
                FROM cash_shifts
                WHERE cash_register_id = @registerId AND status = 'CLOSED'
                
                UNION ALL
                
                -- Open shifts balance (calculated live)
                SELECT 
                    starting_cash + 
                    COALESCE((
                        SELECT SUM(ps.amount) 
                        FROM payment_sales ps 
                        JOIN sales s ON ps.sale_id = s.id 
                        WHERE s.cash_shift_id = cs.id AND ps.reglement = 'Cash' AND s.deleted_at IS NULL
                    ), 0) +
                    COALESCE((
                        SELECT SUM(amount) 
                        FROM cash_transactions 
                        WHERE cash_shift_id = cs.id AND transaction_type = 'FLOAT_IN'
                    ), 0) -
                    COALESCE((
                        SELECT SUM(amount) 
                        FROM cash_transactions 
                        WHERE cash_shift_id = cs.id AND transaction_type = 'CASH_DROP'
                    ), 0) -
                    COALESCE((
                        SELECT SUM(amount) 
                        FROM cash_transactions 
                        WHERE cash_shift_id = cs.id AND transaction_type = 'EXPENSE'
                    ), 0) AS balance
                FROM cash_shifts cs
                WHERE cash_register_id = @registerId AND status = 'OPEN'
            ) as subquery";
        return await _uow.Connection.ExecuteScalarAsync<decimal>(sql, new { registerId }, _uow.Transaction);
    }
}
