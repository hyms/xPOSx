using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class PaymentGatewaySettingsRepository : IPaymentGatewaySettingsRepository
{
    private readonly IUnitOfWork _uow;

    public PaymentGatewaySettingsRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<PaymentGatewaySettings>> GetAllAsync()
    {
        const string sql = "SELECT id, payment_gateway_name as GatewayName, payment_api_key as ApiKey, payment_api_secret as ApiSecret, payment_is_active as IsActive, payment_description as Description, updated_at as UpdatedAt FROM settings";
        return await _uow.Connection.QueryAsync<PaymentGatewaySettings>(sql, null, _uow.Transaction);
    }

    public async Task<PaymentGatewaySettings?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, payment_gateway_name as GatewayName, payment_api_key as ApiKey, payment_api_secret as ApiSecret, payment_is_active as IsActive, payment_description as Description, updated_at as UpdatedAt FROM settings WHERE id = @Id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<PaymentGatewaySettings>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(PaymentGatewaySettings settings)
    {
        const string sql = "UPDATE settings SET payment_gateway_name = @GatewayName, payment_api_key = @ApiKey, payment_api_secret = @ApiSecret, payment_is_active = @IsActive, payment_description = @Description, updated_at = CURRENT_TIMESTAMP WHERE id = (SELECT id FROM settings ORDER BY id LIMIT 1) RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, settings, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(PaymentGatewaySettings settings)
    {
        const string sql = "UPDATE settings SET payment_gateway_name = @GatewayName, payment_api_key = @ApiKey, payment_api_secret = @ApiSecret, payment_is_active = @IsActive, payment_description = @Description, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        settings.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, settings, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "UPDATE settings SET payment_gateway_name = NULL, payment_api_key = NULL, payment_api_secret = NULL, payment_is_active = false, payment_description = NULL WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction) > 0;
    }
}
