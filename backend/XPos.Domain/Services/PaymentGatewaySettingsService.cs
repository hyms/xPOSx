using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class PaymentGatewaySettingsService : IPaymentGatewaySettingsService
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentGatewaySettingsRepository _settingsRepository;

    public PaymentGatewaySettingsService(IUnitOfWork uow, IPaymentGatewaySettingsRepository settingsRepository)
    {
        _uow = uow;
        _settingsRepository = settingsRepository;
    }

    public async Task<IEnumerable<PaymentGatewaySettings>> GetAllAsync()
    {
        return await _settingsRepository.GetAllAsync();
    }

    public async Task<PaymentGatewaySettings?> GetByIdAsync(long id)
    {
        return await _settingsRepository.GetByIdAsync(id);
    }

    public async Task<long> CreateAsync(PaymentGatewaySettings settings)
    {
        _uow.BeginTransaction();
        try
        {
            var id = await _settingsRepository.CreateAsync(settings);
            _uow.Commit();
            return id;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateAsync(PaymentGatewaySettings settings)
    {
        _uow.BeginTransaction();
        try
        {
            var success = await _settingsRepository.UpdateAsync(settings);
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
            var success = await _settingsRepository.DeleteAsync(id);
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
