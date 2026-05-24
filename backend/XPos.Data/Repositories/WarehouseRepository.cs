using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly IUnitOfWork _uow;

    public WarehouseRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        const string sql = "SELECT * FROM warehouses WHERE deleted_at IS NULL";
        return await _uow.Connection.QueryAsync<Warehouse>(sql, null, _uow.Transaction);
    }

    public async Task<Warehouse?> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM warehouses WHERE id = @Id AND deleted_at IS NULL";
        return await _uow.Connection.QueryFirstOrDefaultAsync<Warehouse>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Warehouse warehouse)
    {
        const string sql = @"
            INSERT INTO warehouses (name, city, mobile, email, country, created_at)
            VALUES (@Name, @City, @Mobile, @Email, @Country, @CreatedAt)
            RETURNING id";
        warehouse.CreatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, warehouse, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Warehouse warehouse)
    {
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
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, warehouse, _uow.Transaction);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "UPDATE warehouses SET deleted_at = @DeletedAt WHERE id = @Id";
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow }, _uow.Transaction);
        return rowsAffected > 0;
    }
}
