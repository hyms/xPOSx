using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly string _connectionString;
    public ClientRepository(IConfiguration configuration) { _connectionString = configuration.GetConnectionString("DefaultConnection")!; }
    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<Client>("SELECT * FROM clients WHERE deleted_at IS NULL");
    }

    public async Task<Client?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Client>("SELECT * FROM clients WHERE id = @Id AND deleted_at IS NULL", new { Id = id });
    }

    public async Task<long> CreateAsync(Client client)
    {
        using var connection = CreateConnection();
        const string sql = @"INSERT INTO clients (name, company_name, code, email, city, phone, address, nit_ci, created_at) 
                             VALUES (@Name, @CompanyName, @Code, @Email, @City, @Phone, @Address, @NitCi, @CreatedAt) RETURNING id";
        client.CreatedAt = DateTime.UtcNow;
        return await connection.ExecuteScalarAsync<long>(sql, client);
    }

    public async Task<bool> UpdateAsync(Client client)
    {
        using var connection = CreateConnection();
        const string sql = @"UPDATE clients SET name=@Name, company_name=@CompanyName, code=@Code, email=@Email, city=@City, 
                             phone=@Phone, address=@Address, nit_ci=@NitCi, updated_at=@UpdatedAt WHERE id=@Id";
        client.UpdatedAt = DateTime.UtcNow;
        return await connection.ExecuteAsync(sql, client) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteAsync("UPDATE clients SET deleted_at=@DeletedAt WHERE id=@Id", new { Id = id, DeletedAt = DateTime.UtcNow }) > 0;
    }
}

public class ProviderRepository : IProviderRepository
{
    private readonly string _connectionString;
    public ProviderRepository(IConfiguration configuration) { _connectionString = configuration.GetConnectionString("DefaultConnection")!; }
    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Provider>> GetAllAsync()
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<Provider>("SELECT * FROM providers WHERE deleted_at IS NULL");
    }

    public async Task<Provider?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Provider>("SELECT * FROM providers WHERE id = @Id AND deleted_at IS NULL", new { Id = id });
    }

    public async Task<long> CreateAsync(Provider provider)
    {
        using var connection = CreateConnection();
        const string sql = @"INSERT INTO providers (name, code, email, phone, country, city, address, created_at) 
                             VALUES (@Name, @Code, @Email, @Phone, @Country, @City, @Address, @CreatedAt) RETURNING id";
        provider.CreatedAt = DateTime.UtcNow;
        return await connection.ExecuteScalarAsync<long>(sql, provider);
    }

    public async Task<bool> UpdateAsync(Provider provider)
    {
        using var connection = CreateConnection();
        const string sql = @"UPDATE providers SET name=@Name, code=@Code, email=@Email, phone=@Phone, country=@Country, 
                             city=@City, address=@Address, updated_at=@UpdatedAt WHERE id=@Id";
        provider.UpdatedAt = DateTime.UtcNow;
        return await connection.ExecuteAsync(sql, provider) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteAsync("UPDATE providers SET deleted_at=@DeletedAt WHERE id=@Id", new { Id = id, DeletedAt = DateTime.UtcNow }) > 0;
    }
}
