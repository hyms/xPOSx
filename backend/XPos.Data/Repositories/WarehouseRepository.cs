using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly string _connectionString;

    public WarehouseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM warehouses WHERE deleted_at IS NULL";
        return await connection.QueryAsync<Warehouse>(sql);
    }

    public async Task<Warehouse?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM warehouses WHERE id = @Id AND deleted_at IS NULL";
        return await connection.QueryFirstOrDefaultAsync<Warehouse>(sql, new { Id = id });
    }

    public async Task<long> CreateAsync(Warehouse warehouse)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO warehouses (name, city, mobile, email, country, created_at)
            VALUES (@Name, @City, @Mobile, @Email, @Country, @CreatedAt)
            RETURNING id";
        warehouse.CreatedAt = DateTime.UtcNow;
        return await connection.ExecuteScalarAsync<long>(sql, warehouse);
    }

    public async Task<bool> UpdateAsync(Warehouse warehouse)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE warehouses 
            SET name = @Name, 
                city = @City, 
                mobile = @Mobile, 
                email = @Email, 
                country = @Country, 
                updated_at = @UpdatedAt
            WHERE id = @Id";
        warehouse.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, warehouse);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE warehouses SET deleted_at = @DeletedAt WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }
}
