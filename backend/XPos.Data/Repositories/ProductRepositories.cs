using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using System.Data;
using System.Text;

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
    private readonly ICurrentUserService _currentUserService;

    public ProductRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder(@"
            SELECT p.id, p.code, p.name, p.cost, p.price, p.category_id, p.unit_id, p.unit_sale_id, p.unit_purchase_id,
                   p.tax_net, p.tax_method, p.note, p.stock_alert, p.is_variant, p.not_selling, p.is_active, p.image,
                   (SELECT COALESCE(SUM(pw.qty), 0) FROM product_warehouse pw WHERE pw.product_id = p.id AND pw.warehouse_id = @activeWarehouseId) as stock,
                   c.id, c.code, c.name,
                   u.id, u.name, u.short_name, u.base_unit, u.operator, u.operator_value
            FROM products p 
            LEFT JOIN categories c ON p.category_id = c.id 
            LEFT JOIN units u ON p.unit_id = u.id 
            WHERE p.deleted_at IS NULL");
        
        var parameters = new DynamicParameters();
        parameters.Add("activeWarehouseId", activeWarehouseId);

        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND p.id IN (SELECT DISTINCT product_id FROM product_warehouse WHERE warehouse_id = @activeWarehouseId)");
        }

        var products = await _uow.Connection.QueryAsync<Product, Category, Unit, Product>(sqlBuilder.ToString(), 
            (p, c, u) => { p.Category = c; p.Unit = u; return p; }, 
            parameters, splitOn: "id,id", transaction: _uow.Transaction);

        // The stock subquery already filters by activeWarehouseId. If hasAllAccess is true, 
        // the stock is still for the active warehouse, but the product list is global.
        // If we want a global sum of stock, we'd remove the warehouse_id filter from the subquery
        // based on hasAllAccess, but for now, we assume stock shown is always for the active warehouse.

        return products;
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder(@"
            SELECT p.id, p.code, p.name, p.cost, p.price, p.category_id, p.unit_id, p.unit_sale_id, p.unit_purchase_id,
                   p.tax_net, p.tax_method, p.note, p.stock_alert, p.is_variant, p.not_selling, p.is_active, p.image,
                   (SELECT COALESCE(SUM(pw.qty), 0) FROM product_warehouse pw WHERE pw.product_id = p.id AND pw.warehouse_id = @activeWarehouseId) as stock,
                   c.id, c.code, c.name,
                   u.id, u.name, u.short_name, u.base_unit, u.operator, u.operator_value
            FROM products p 
            LEFT JOIN categories c ON p.category_id = c.id 
            LEFT JOIN units u ON p.unit_id = u.id 
            WHERE p.id = @id AND p.deleted_at IS NULL");
        
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("activeWarehouseId", activeWarehouseId);

        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND p.id IN (SELECT DISTINCT product_id FROM product_warehouse WHERE warehouse_id = @activeWarehouseId)");
        }

        var product = (await _uow.Connection.QueryAsync<Product, Category, Unit, Product>(sqlBuilder.ToString(), 
            (p, c, u) => { p.Category = c; p.Unit = u; return p; }, 
            parameters, splitOn: "id,id", transaction: _uow.Transaction)).FirstOrDefault();

        return product;
    }

    public async Task<long> CreateAsync(Product product)
    {
        // Products are conceptually global, and their stock is warehouse-specific.
        // This method only creates the product entry, not its initial stock in any warehouse.
        // Initial stock creation (if any) would be handled via inventory operations after product creation.

        const string sql = @"
            INSERT INTO products (code, name, cost, price, category_id, unit_id, unit_sale_id, unit_purchase_id, tax_net, tax_method, note, stock_alert, is_variant, not_selling, is_active, image, created_at) 
            VALUES (@Code, @Name, @Cost, @Price, @CategoryId, @UnitId, @UnitSaleId, @UnitPurchaseId, @TaxNet, @TaxMethod, @Note, @StockAlert, @IsVariant, @NotSelling, @IsActive, @Image, CURRENT_TIMESTAMP) 
            RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, product, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder(@"
            UPDATE products SET 
                code = @Code, name = @Name, cost = @Cost, price = @Price, category_id = @CategoryId, 
                unit_id = @UnitId, unit_sale_id = @UnitSaleId, unit_purchase_id = @UnitPurchaseId, 
                tax_net = @TaxNet, tax_method = @TaxMethod, note = @Note, stock_alert = @StockAlert, 
                is_variant = @IsVariant, not_selling = @NotSelling, is_active = @IsActive, 
                image = @Image, updated_at = CURRENT_TIMESTAMP 
            WHERE id = @Id");
        
        var parameters = new DynamicParameters(product);

        // For product updates, ensure the product being updated is visible/accessible to the user
        // The check is implicit because the frontend should only display products available in the active warehouse
        // Or if the user has all-access, they can update any product.
        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND id IN (SELECT DISTINCT product_id FROM product_warehouse WHERE warehouse_id = @activeWarehouseId)");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sqlBuilder.ToString(), parameters, _uow.Transaction) > 0;
    }

    public async Task<bool> UpdateCostAsync(long productId, decimal newCost)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder("UPDATE products SET cost = @Cost WHERE id = @Id");
        var parameters = new DynamicParameters(new { Cost = newCost, Id = productId });

        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND id IN (SELECT DISTINCT product_id FROM product_warehouse WHERE warehouse_id = @activeWarehouseId)");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sqlBuilder.ToString(), parameters, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder("UPDATE products SET deleted_at = CURRENT_TIMESTAMP WHERE id = @id");
        var parameters = new DynamicParameters(new { id });

        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND id IN (SELECT DISTINCT product_id FROM product_warehouse WHERE warehouse_id = @activeWarehouseId)");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.ExecuteAsync(sqlBuilder.ToString(), parameters, _uow.Transaction) > 0;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var sqlBuilder = new StringBuilder("SELECT * FROM products WHERE category_id = @categoryId AND deleted_at IS NULL");
        var parameters = new DynamicParameters();
        parameters.Add("categoryId", categoryId);

        if (!hasAllAccess)
        {
            sqlBuilder.Append(" AND id IN (SELECT DISTINCT product_id FROM product_warehouse WHERE warehouse_id = @activeWarehouseId)");
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        return await _uow.Connection.QueryAsync<Product>(sqlBuilder.ToString(), parameters, _uow.Transaction);
    }
}
