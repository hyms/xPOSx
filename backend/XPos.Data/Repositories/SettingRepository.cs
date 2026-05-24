using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class SettingRepository : ISettingRepository
{
    private readonly IUnitOfWork _uow;

    public SettingRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Setting?> GetAsync()
    {
        const string sql = "SELECT * FROM settings ORDER BY id LIMIT 1";
        return await _uow.Connection.QueryFirstOrDefaultAsync<Setting>(sql, null, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Setting setting)
    {
        const string sql = @"
            UPDATE settings 
            SET company_name = @CompanyName, 
                email = @Email, 
                company_phone = @CompanyPhone, 
                company_address = @CompanyAddress, 
                logo = @Logo, 
                version = @Version, 
                days = @Days,
                currency_code = @CurrencyCode,
                currency_symbol = @CurrencySymbol,
                currency_name = @CurrencyName,
                mail_host = @MailHost,
                mail_port = @MailPort,
                mail_username = @MailUsername,
                mail_password = @MailPassword,
                mail_encryption = @MailEncryption,
                mail_from_address = @MailFromAddress,
                mail_from_name = @MailFromName,
                sms_gateway_name = @SmsGatewayName,
                sms_api_key = @SmsApiKey,
                sms_api_secret = @SmsApiSecret,
                sms_sender_id = @SmsSenderId,
                sms_is_active = @SmsIsActive,
                sms_description = @SmsDescription,
                payment_gateway_name = @PaymentGatewayName,
                payment_api_key = @PaymentApiKey,
                payment_api_secret = @PaymentApiSecret,
                payment_is_active = @PaymentIsActive,
                payment_description = @PaymentDescription,
                siat_token = @SiatToken,
                siat_certificate = @SiatCertificate,
                siat_environment = @SiatEnvironment,
                siat_modality = @SiatModality,
                siat_emission_type = @SiatEmissionType,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        setting.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, setting, _uow.Transaction);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateMediaAsync(string type, string path)
    {
        string column = type.ToLower() == "logo" ? "logo" : "favicon";
        string sql = $@"
            UPDATE settings 
            SET {column} = @Path, 
                settings_version = settings_version + 1,
                updated_at = @UpdatedAt
            WHERE id = (SELECT id FROM settings ORDER BY id LIMIT 1)";
        
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { 
            Path = path, 
            UpdatedAt = DateTime.UtcNow 
        }, _uow.Transaction);
        return rowsAffected > 0;
    }
}
