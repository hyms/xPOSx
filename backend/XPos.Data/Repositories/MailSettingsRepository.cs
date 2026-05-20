using Dapper;
using System.Data;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class MailSettingsRepository : IMailSettingsRepository
{
    private readonly IUnitOfWork _uow;

    public MailSettingsRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<MailSettings?> GetAsync()
    {
        const string sql = "SELECT id, mail_host as Host, mail_port as Port, mail_username as Username, mail_password as Password, mail_encryption as Encryption, mail_from_address as FromAddress, mail_from_name as FromName, updated_at as UpdatedAt FROM settings ORDER BY id LIMIT 1";
        return await _uow.Connection.QueryFirstOrDefaultAsync<MailSettings>(sql, null, _uow.Transaction);
    }

    public async Task<long> CreateAsync(MailSettings mailSettings)
    {
        // Settings table already has a row, so we usually update it. 
        // But if we must 'Create', we update the existing one or throw.
        // For compatibility with previous logic, we'll update the first row.
        const string sql = "UPDATE settings SET mail_host = @Host, mail_port = @Port, mail_username = @Username, mail_password = @Password, mail_encryption = @Encryption, mail_from_address = @FromAddress, mail_from_name = @FromName, updated_at = CURRENT_TIMESTAMP WHERE id = (SELECT id FROM settings ORDER BY id LIMIT 1) RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, mailSettings, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(MailSettings mailSettings)
    {
        const string sql = "UPDATE settings SET mail_host = @Host, mail_port = @Port, mail_username = @Username, mail_password = @Password, mail_encryption = @Encryption, mail_from_address = @FromAddress, mail_from_name = @FromName, updated_at = CURRENT_TIMESTAMP WHERE id = (SELECT id FROM settings ORDER BY id LIMIT 1)";
        mailSettings.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, mailSettings, _uow.Transaction) > 0;
    }
}
