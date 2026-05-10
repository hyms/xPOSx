using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IExpenseRepository
{
    Task<IEnumerable<ExpenseReadDto>> GetAllAsync();
    Task<Expense?> GetByIdAsync(long id);
    Task<long> CreateAsync(Expense expense);
    Task<bool> UpdateAsync(Expense expense);
    Task<bool> DeleteAsync(long id, long userId);
}

public interface IExpenseCategoryRepository
{
    Task<IEnumerable<ExpenseCategory>> GetAllAsync();
    Task<ExpenseCategory?> GetByIdAsync(long id);
    Task<long> CreateAsync(ExpenseCategory category);
    Task<bool> UpdateAsync(ExpenseCategory category);
    Task<bool> DeleteAsync(long id, long userId);
}
