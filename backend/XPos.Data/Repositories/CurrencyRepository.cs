using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly IUnitOfWork _uow;

    public CurrencyRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        const string sql = "SELECT id, code, symbol, name, is_main as IsMain FROM currencies";
        return await _uow.Connection.QueryAsync<Currency>(sql, null, _uow.Transaction);
    }

    public async Task<Currency?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, code, symbol, name, is_main as IsMain FROM currencies WHERE id = @Id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<Currency>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(Currency currency)
    {
        const string sql = "INSERT INTO currencies (code, symbol, name, is_main) VALUES (@Code, @Symbol, @Name, @IsMain) RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, currency, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Currency currency)
    {
        const string sql = "UPDATE currencies SET code = @Code, symbol = @Symbol, name = @Name, is_main = @IsMain WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, currency, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM currencies WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction) > 0;
    }
}
