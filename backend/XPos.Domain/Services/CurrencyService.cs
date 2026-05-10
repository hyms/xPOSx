using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrencyRepository _currencyRepository;

    public CurrencyService(IUnitOfWork uow, ICurrencyRepository currencyRepository)
    {
        _uow = uow;
        _currencyRepository = currencyRepository;
    }

    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        return await _currencyRepository.GetAllAsync();
    }

    public async Task<Currency?> GetByIdAsync(long id)
    {
        return await _currencyRepository.GetByIdAsync(id);
    }

    public async Task<long> CreateAsync(Currency currency)
    {
        _uow.BeginTransaction();
        try
        {
            var id = await _currencyRepository.CreateAsync(currency);
            _uow.Commit();
            return id;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Currency currency)
    {
        _uow.BeginTransaction();
        try
        {
            var success = await _currencyRepository.UpdateAsync(currency);
            _uow.Commit();
            return success;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        _uow.BeginTransaction();
        try
        {
            var success = await _currencyRepository.DeleteAsync(id);
            _uow.Commit();
            return success;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
