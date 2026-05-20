using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

using System.Text;

namespace XPos.Data.Repositories;


public class ExpenseRepository : IExpenseRepository
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public ExpenseRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ExpenseReadDto>> GetAllAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sql = new StringBuilder(@"
            SELECT e.id, e.ref, e.date::timestamp as Date, e.details, e.amount,
                   ec.name as CategoryName, w.name as WarehouseName
            FROM expenses e
            JOIN expense_categories ec ON e.expense_category_id = ec.id
            JOIN warehouses w ON e.warehouse_id = w.id
            WHERE e.deleted_at IS NULL");

        var parameters = new DynamicParameters();

        if (!hasAllAccess)
        {
            sql.Append(" AND e.warehouse_id = @activeWarehouseId");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        sql.Append(" ORDER BY e.date DESC");
        
        return await _uow.Connection.QueryAsync<ExpenseReadDto>(sql.ToString(), parameters, _uow.Transaction);
    }

    public async Task<Expense?> GetByIdAsync(long id)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = "SELECT * FROM expenses WHERE id = @id AND deleted_at IS NULL";
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.QueryFirstOrDefaultAsync<Expense>(sql, parameters, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Expense expense)
    {
        const string sql = @"
            INSERT INTO expenses (user_id, date, ref, expense_category_id, warehouse_id, details, amount, created_at, created_by)
            VALUES (@UserId, @Date, @Ref, @ExpenseCategoryId, @WarehouseId, @Details, @Amount, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, expense, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Expense expense)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            UPDATE expenses SET
                date = @Date,
                ref = @Ref,
                expense_category_id = @ExpenseCategoryId,
                warehouse_id = @WarehouseId,
                details = @Details,
                amount = @Amount,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @UpdatedBy
            WHERE id = @Id AND deleted_at IS NULL";
        
        var parameters = new DynamicParameters(expense);

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

        string sql = "UPDATE expenses SET deleted_at = CURRENT_TIMESTAMP, updated_by = @userId WHERE id = @id AND deleted_at IS NULL";
        var parameters = new DynamicParameters(new { id, userId });

        if (!hasAllAccess)
        {
            sql += " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sql, parameters, _uow.Transaction) > 0;
    }
}

public class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public ExpenseCategoryRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ExpenseCategory>> GetAllAsync()
    {
        const string sql = "SELECT * FROM expense_categories WHERE deleted_at IS NULL ORDER BY name ASC";
        return await _uow.Connection.QueryAsync<ExpenseCategory>(sql, null, _uow.Transaction);
    }

    public async Task<ExpenseCategory?> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM expense_categories WHERE id = @id AND deleted_at IS NULL";
        return await _uow.Connection.QueryFirstOrDefaultAsync<ExpenseCategory>(sql, new { id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(ExpenseCategory category)
    {
        const string sql = @"
            INSERT INTO expense_categories (user_id, name, description, created_at, created_by)
            VALUES (@UserId, @Name, @Description, CURRENT_TIMESTAMP, @CreatedBy)
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, category, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(ExpenseCategory category)
    {
        const string sql = @"
            UPDATE expense_categories SET 
                name = @Name,
                description = @Description,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @UpdatedBy
            WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, category, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        const string sql = "UPDATE expense_categories SET deleted_at = CURRENT_TIMESTAMP, deleted_by = @userId WHERE id = @id";
        return await _uow.Connection.ExecuteAsync(sql, new { id, userId }, _uow.Transaction) > 0;
    }
}
