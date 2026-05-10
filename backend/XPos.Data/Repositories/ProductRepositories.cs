using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;

namespace XPos.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly string _connectionString;
    public CategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();
    }
    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<Category>("SELECT id, code, name FROM categories WHERE deleted_at IS NULL");
    }

    public async Task<Category?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Category>("SELECT id, code, name FROM categories WHERE id = @id AND deleted_at IS NULL", new { id });
    }

    public async Task<long> CreateAsync(Category category)
    {
        using var connection = CreateConnection();
        const string sql = "INSERT INTO categories (code, name, created_at) VALUES (@Code, @Name, CURRENT_TIMESTAMP) RETURNING id";
        return await connection.ExecuteScalarAsync<long>(sql, category);
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE categories SET code = @Code, name = @Name, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        return await connection.ExecuteAsync(sql, category) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE categories SET deleted_at = CURRENT_TIMESTAMP WHERE id = @id";
        return await connection.ExecuteAsync(sql, new { id }) > 0;
    }
}

public class UnitRepository : IUnitRepository
{
    private readonly IUnitOfWork _uow;

    public UnitRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Unit>> GetAllAsync()
    {
        return await _uow.Connection.QueryAsync<Unit>("SELECT id, name, short_name as ShortName, base_unit as BaseUnit, operator, operator_value as OperatorValue FROM units WHERE deleted_at IS NULL", null, _uow.Transaction);
    }

    public async Task<Unit?> GetByIdAsync(long id)
    {
        return await _uow.Connection.QueryFirstOrDefaultAsync<Unit>("SELECT id, name, short_name as ShortName, base_unit as BaseUnit, operator, operator_value as OperatorValue FROM units WHERE id = @id AND deleted_at IS NULL", new { id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Unit unit)
    {
        const string sql = "INSERT INTO units (name, short_name, base_unit, operator, operator_value, created_at) VALUES (@Name, @ShortName, @BaseUnit, @Operator, @OperatorValue, CURRENT_TIMESTAMP) RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, unit, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Unit unit)
    {
        const string sql = "UPDATE units SET name = @Name, short_name = @ShortName, base_unit = @BaseUnit, operator = @Operator, operator_value = @OperatorValue, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, unit, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "UPDATE units SET deleted_at = CURRENT_TIMESTAMP WHERE id = @id";
        return await _uow.Connection.ExecuteAsync(sql, new { id }, _uow.Transaction) > 0;
    }
}

public class ProductRepository : IProductRepository
{
    private readonly IUnitOfWork _uow;

    public ProductRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string sql = @"
            SELECT p.id, p.code, p.name, p.cost, p.price, p.category_id, p.unit_id, p.unit_sale_id, p.unit_purchase_id,
                   p.tax_net, p.tax_method, p.note, p.stock_alert, p.is_variant, p.not_selling, p.is_active, p.image,
                   (SELECT SUM(qty) FROM product_warehouse pw WHERE pw.product_id = p.id) as stock,
                   c.id, c.code, c.name,
                   u.id, u.name, u.short_name, u.base_unit, u.operator, u.operator_value
            FROM products p 
            LEFT JOIN categories c ON p.category_id = c.id 
            LEFT JOIN units u ON p.unit_id = u.id 
            WHERE p.deleted_at IS NULL";
        
        return await _uow.Connection.QueryAsync<Product, Category, Unit, Product>(sql, 
            (p, c, u) => { p.Category = c; p.Unit = u; return p; }, 
            splitOn: "id,id", transaction: _uow.Transaction);
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM products WHERE id = @id AND deleted_at IS NULL";
        return await _uow.Connection.QueryFirstOrDefaultAsync<Product>(sql, new { id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Product product)
    {
        const string sql = @"
            INSERT INTO products (code, name, cost, price, category_id, unit_id, unit_sale_id, unit_purchase_id, tax_net, tax_method, note, stock_alert, is_variant, not_selling, is_active, image, created_at) 
            VALUES (@Code, @Name, @Cost, @Price, @CategoryId, @UnitId, @UnitSaleId, @UnitPurchaseId, @TaxNet, @TaxMethod, @Note, @StockAlert, @IsVariant, @NotSelling, @IsActive, @Image, CURRENT_TIMESTAMP) 
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, product, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        const string sql = @"
            UPDATE products SET 
                code = @Code, name = @Name, cost = @Cost, price = @Price, category_id = @CategoryId, 
                unit_id = @UnitId, unit_sale_id = @UnitSaleId, unit_purchase_id = @UnitPurchaseId, 
                tax_net = @TaxNet, tax_method = @TaxMethod, note = @Note, stock_alert = @StockAlert, 
                is_variant = @IsVariant, not_selling = @NotSelling, is_active = @IsActive, 
                image = @Image, updated_at = CURRENT_TIMESTAMP 
            WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, product, _uow.Transaction) > 0;
    }

    public async Task<bool> UpdateCostAsync(long productId, double newCost)
    {
        const string sql = "UPDATE products SET cost = @Cost WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, new { Cost = newCost, Id = productId }, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "UPDATE products SET deleted_at = CURRENT_TIMESTAMP WHERE id = @id";
        return await _uow.Connection.ExecuteAsync(sql, new { id }, _uow.Transaction) > 0;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId)
    {
        return await _uow.Connection.QueryAsync<Product>("SELECT * FROM products WHERE category_id = @categoryId AND deleted_at IS NULL", new { categoryId }, _uow.Transaction);
    }
}
