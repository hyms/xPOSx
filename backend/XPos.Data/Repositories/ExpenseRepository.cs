using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;

namespace XPos.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly string _connectionString;
    public ExpenseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();
    }
    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<ExpenseReadDto>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT e.id, e.ref, e.date::timestamp as Date, e.details, e.amount,
                   ec.name as CategoryName, w.name as WarehouseName
            FROM expenses e
            JOIN expense_categories ec ON e.expense_category_id = ec.id
            JOIN warehouses w ON e.warehouse_id = w.id
            WHERE e.deleted_at IS NULL
            ORDER BY e.date DESC";
        return await connection.QueryAsync<ExpenseReadDto>(sql);
    }

    public async Task<Expense?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM expenses WHERE id = @id AND deleted_at IS NULL";
        return await connection.QueryFirstOrDefaultAsync<Expense>(sql, new { id });
    }

    public async Task<long> CreateAsync(Expense expense)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO expenses (user_id, date, ref, expense_category_id, warehouse_id, details, amount, created_at, created_by)
            VALUES (@UserId, @Date, @Ref, @ExpenseCategoryId, @WarehouseId, @Details, @Amount, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        return await connection.ExecuteScalarAsync<long>(sql, expense);
    }

    public async Task<bool> UpdateAsync(Expense expense)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE expenses SET 
                date = @Date,
                ref = @Ref,
                expense_category_id = @ExpenseCategoryId,
                warehouse_id = @WarehouseId,
                details = @Details,
                amount = @Amount,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @UpdatedBy
            WHERE id = @Id";
        return await connection.ExecuteAsync(sql, expense) > 0;
    }

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE expenses SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @userId WHERE id = @id";
        return await connection.ExecuteAsync(sql, new { id, userId }) > 0;
    }
}

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly string _connectionString;
    public ExpenseCategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();
    }
    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<ExpenseCategory>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM expense_categories WHERE deleted_at IS NULL ORDER BY name ASC";
        return await connection.QueryAsync<ExpenseCategory>(sql);
    }

    public async Task<ExpenseCategory?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM expense_categories WHERE id = @id AND deleted_at IS NULL";
        return await connection.QueryFirstOrDefaultAsync<ExpenseCategory>(sql, new { id });
    }

    public async Task<long> CreateAsync(ExpenseCategory category)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO expense_categories (user_id, name, description, created_at, created_by)
            VALUES (@UserId, @Name, @Description, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        return await connection.ExecuteScalarAsync<long>(sql, category);
    }

    public async Task<bool> UpdateAsync(ExpenseCategory category)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE expense_categories SET 
                name = @Name,
                description = @Description,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @UpdatedBy
            WHERE id = @Id";
        return await connection.ExecuteAsync(sql, category) > 0;
    }

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE expense_categories SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @userId WHERE id = @id";
        return await connection.ExecuteAsync(sql, new { id, userId }) > 0;
    }
}
