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
        const string sql = "SELECT id, gateway_name as GatewayName, api_key as ApiKey, api_secret as ApiSecret, is_active as IsActive, description as Description, created_at as CreatedAt, updated_at as UpdatedAt FROM payment_gateway_settings";
        return await _uow.Connection.QueryAsync<PaymentGatewaySettings>(sql, null, _uow.Transaction);
    }

    public async Task<PaymentGatewaySettings?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, gateway_name as GatewayName, api_key as ApiKey, api_secret as ApiSecret, is_active as IsActive, description as Description, created_at as CreatedAt, updated_at as UpdatedAt FROM payment_gateway_settings WHERE id = @Id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<PaymentGatewaySettings>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(PaymentGatewaySettings settings)
    {
        const string sql = "INSERT INTO payment_gateway_settings (gateway_name, api_key, api_secret, is_active, description, created_at) VALUES (@GatewayName, @ApiKey, @ApiSecret, @IsActive, @Description, CURRENT_TIMESTAMP) RETURNING id";
        settings.CreatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, settings, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(PaymentGatewaySettings settings)
    {
        const string sql = "UPDATE payment_gateway_settings SET gateway_name = @GatewayName, api_key = @ApiKey, api_secret = @ApiSecret, is_active = @IsActive, description = @Description, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        settings.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, settings, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM payment_gateway_settings WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction) > 0;
    }
}
