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
        const string sql = "SELECT id, gateway_name as GatewayName, api_key as ApiKey, api_secret as ApiSecret, sender_id as SenderId, is_active as IsActive, description as Description, created_at as CreatedAt, updated_at as UpdatedAt FROM sms_settings";
        return await _uow.Connection.QueryAsync<SmsSettings>(sql, null, _uow.Transaction);
    }

    public async Task<SmsSettings?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, gateway_name as GatewayName, api_key as ApiKey, api_secret as ApiSecret, sender_id as SenderId, is_active as IsActive, description as Description, created_at as CreatedAt, updated_at as UpdatedAt FROM sms_settings WHERE id = @Id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<SmsSettings>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<long> CreateAsync(SmsSettings settings)
    {
        const string sql = "INSERT INTO sms_settings (gateway_name, api_key, api_secret, sender_id, is_active, description, created_at) VALUES (@GatewayName, @ApiKey, @ApiSecret, @SenderId, @IsActive, @Description, CURRENT_TIMESTAMP) RETURNING id";
        settings.CreatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, settings, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(SmsSettings settings)
    {
        const string sql = "UPDATE sms_settings SET gateway_name = @GatewayName, api_key = @ApiKey, api_secret = @ApiSecret, sender_id = @SenderId, is_active = @IsActive, description = @Description, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        settings.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, settings, _uow.Transaction) > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM sms_settings WHERE id = @Id";
        return await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction) > 0;
    }
}
