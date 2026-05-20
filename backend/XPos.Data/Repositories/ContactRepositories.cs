using Dapper;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly IUnitOfWork _uow;

    public ClientRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        const string sql = @"SELECT id, name, company_name AS CompanyName, code, email, city, phone, address, 
                             nit_ci AS NitCi, created_at AS CreatedAt, updated_at AS UpdatedAt, deleted_at AS DeletedAt 
                             FROM clients WHERE deleted_at IS NULL";
        return await _uow.Connection.QueryAsync<Client>(sql, null, _uow.Transaction);
    }

    public async Task<Client?> GetByIdAsync(long id)
    {
        return await _uow.Connection.QueryFirstOrDefaultAsync<Client>("SELECT * FROM clients WHERE id = @Id AND deleted_at IS NULL", new { Id = id }, _uow.Transaction);
    }

    public async Task<Client?> GetByNitAsync(string nit)
    {
        const string sql = @"SELECT id, name, company_name AS CompanyName, code, email, city, phone, address, 
                             nit_ci AS NitCi, created_at AS CreatedAt, updated_at AS UpdatedAt, deleted_at AS DeletedAt 
                             FROM clients WHERE nit_ci = @Nit AND deleted_at IS NULL LIMIT 1";
        return await _uow.Connection.QueryFirstOrDefaultAsync<Client>(sql, new { Nit = nit }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Client client)
    {
        const string sql = @"INSERT INTO clients (name, company_name, code, email, city, phone, address, nit_ci, created_at) 
                             VALUES (@Name, @CompanyName, @Code, @Email, @City, @Phone, @Address, @NitCi, @CreatedAt) RETURNING id";
        client.CreatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, client, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Client client)
    {
        const string sql = @"UPDATE clients SET name=@Name, company_name=@CompanyName, code=@Code, email=@Email, city=@City, 
                             phone=@Phone, address=@Address, nit_ci=@NitCi, updated_at=@UpdatedAt WHERE id=@Id";
        client.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, client, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        return await _uow.Connection.ExecuteAsync("UPDATE clients SET deleted_at=@DeletedAt WHERE id=@Id", new { Id = id, DeletedAt = DateTime.UtcNow }, _uow.Transaction) > 0;
    }
}

public class ProviderRepository : IProviderRepository
{
    private readonly IUnitOfWork _uow;

    public ProviderRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Provider>> GetAllAsync()
    {
        return await _uow.Connection.QueryAsync<Provider>("SELECT * FROM providers WHERE deleted_at IS NULL", null, _uow.Transaction);
    }

    public async Task<Provider?> GetByIdAsync(long id)
    {
        return await _uow.Connection.QueryFirstOrDefaultAsync<Provider>("SELECT * FROM providers WHERE id = @Id AND deleted_at IS NULL", new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Provider provider)
    {
        const string sql = @"INSERT INTO providers (name, code, email, phone, country, city, address, created_at) 
                             VALUES (@Name, @Code, @Email, @Phone, @Country, @City, @Address, @CreatedAt) RETURNING id";
        provider.CreatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, provider, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Provider provider)
    {
        const string sql = @"UPDATE providers SET name=@Name, code=@Code, email=@Email, phone=@Phone, country=@Country, 
                             city=@City, address=@Address, updated_at=@UpdatedAt WHERE id=@Id";
        provider.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, provider, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        return await _uow.Connection.ExecuteAsync("UPDATE providers SET deleted_at=@DeletedAt WHERE id=@Id", new { Id = id, DeletedAt = DateTime.UtcNow }, _uow.Transaction) > 0;
    }
}
