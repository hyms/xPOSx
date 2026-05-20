using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class SmsSettingsRepository : ISmsSettingsRepository
{
    private readonly IUnitOfWork _uow;

    public SmsSettingsRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<SmsSettings>> GetAllAsync()
    {
        const string sql = "SELECT id, sms_gateway_name as GatewayName, sms_api_key as ApiKey, sms_api_secret as ApiSecret, sms_sender_id as SenderId, sms_is_active as IsActive, sms_description as Description, updated_at as UpdatedAt FROM settings";
        return await _uow.Connection.QueryAsync<SmsSettings>(sql, null, _uow.Transaction);
    }

    public async Task<SmsSettings?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, sms_gateway_name as GatewayName, sms_api_key as ApiKey, sms_api_secret as ApiSecret, sms_sender_id as SenderId, sms_is_active as IsActive, sms_description as Description, updated_at as UpdatedAt FROM settings WHERE id = @Id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<SmsSettings>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(SmsSettings settings)
    {
        const string sql = "UPDATE settings SET sms_gateway_name = @GatewayName, sms_api_key = @ApiKey, sms_api_secret = @ApiSecret, sms_sender_id = @SenderId, sms_is_active = @IsActive, sms_description = @Description, updated_at = CURRENT_TIMESTAMP WHERE id = (SELECT id FROM settings ORDER BY id LIMIT 1) RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, settings, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(SmsSettings settings)
    {
        const string sql = "UPDATE settings SET sms_gateway_name = @GatewayName, sms_api_key = @ApiKey, sms_api_secret = @ApiSecret, sms_sender_id = @SenderId, sms_is_active = @IsActive, sms_description = @Description, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        settings.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, settings, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "UPDATE settings SET sms_gateway_name = NULL, sms_api_key = NULL, sms_api_secret = NULL, sms_sender_id = NULL, sms_is_active = false, sms_description = NULL WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction) > 0;
    }
}
