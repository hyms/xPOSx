using Dapper;
using Dapper.Contrib.Extensions;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CurrencySetting?> GetAsync()
    {
        const string sql = "SELECT \"Id\", \"Code\", \"Symbol\" FROM public.\"CurrencySettings\" LIMIT 1";
        return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<CurrencySetting>(sql, transaction: _unitOfWork.Transaction);
    }

    public async Task<CurrencySetting?> GetByIdAsync(long id)
    {
        const string sql = "SELECT \"Id\", \"Code\", \"Symbol\" FROM public.\"CurrencySettings\" WHERE \"Id\" = @id";
        return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<CurrencySetting>(sql, new { id }, _unitOfWork.Transaction);
    }

    public async Task<List<CurrencySetting>> GetAllAsync()
    {
        const string sql = "SELECT \"Id\", \"Code\", \"Symbol\" FROM public.\"CurrencySettings\"";
        var result = await _unitOfWork.Connection.QueryAsync<CurrencySetting>(sql, transaction: _unitOfWork.Transaction);
        return result.ToList();
    }

    public async Task<long> CreateAsync(CurrencySetting currency)
    {
        const string sql = @"
            INSERT INTO public.""CurrencySettings"" (""Code"", ""Symbol"")
            VALUES (@Code, @Symbol)
            RETURNING ""Id""";
        return await _unitOfWork.Connection.ExecuteScalarAsync<long>(sql, currency, _unitOfWork.Transaction);
    }

    public async Task<bool> UpdateAsync(CurrencySetting currency)
    {
        const string sql = @"
            UPDATE public.""CurrencySettings""
            SET ""Code"" = @Code, ""Symbol"" = @Symbol
            WHERE ""Id"" = @Id";
        return await _unitOfWork.Connection.ExecuteAsync(sql, currency, _unitOfWork.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM public.\"CurrencySettings\" WHERE \"Id\" = @id";
        return await _unitOfWork.Connection.ExecuteAsync(sql, new { id }, _unitOfWork.Transaction) > 0;
    }
}
