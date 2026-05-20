using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(long id);
    Task<long> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(long id);
}

public interface IUnitRepository
{
    Task<IEnumerable<Unit>> GetAllAsync();
    Task<Unit?> GetByIdAsync(long id);
    Task<long> CreateAsync(Unit unit);
    Task<bool> UpdateAsync(Unit unit);
    Task<bool> DeleteAsync(long id);
}

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(long id);
    Task<long> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> UpdateCostAsync(long productId, decimal newCost);
    Task<bool> DeleteAsync(long id);
    Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId);
}
