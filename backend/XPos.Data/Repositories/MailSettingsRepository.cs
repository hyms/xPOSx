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
        const string sql = "SELECT id, host, port, username, password, encryption, from_address as FromAddress, from_name as FromName, created_at as CreatedAt, updated_at as UpdatedAt FROM mail_settings ORDER BY id DESC LIMIT 1";
        return await _uow.Connection.QueryFirstOrDefaultAsync<MailSettings>(sql, null, _uow.Transaction);
    }

    public async Task<long> CreateAsync(MailSettings mailSettings)
    {
        const string sql = "INSERT INTO mail_settings (host, port, username, password, encryption, from_address, from_name, created_at) VALUES (@Host, @Port, @Username, @Password, @Encryption, @FromAddress, @FromName, CURRENT_TIMESTAMP) RETURNING id";
        mailSettings.CreatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, mailSettings, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(MailSettings mailSettings)
    {
        const string sql = "UPDATE mail_settings SET host = @Host, port = @Port, username = @Username, password = @Password, encryption = @Encryption, from_address = @FromAddress, from_name = @FromName, updated_at = CURRENT_TIMESTAMP WHERE id = @Id";
        mailSettings.UpdatedAt = DateTime.UtcNow;
        return await _uow.Connection.ExecuteAsync(sql, mailSettings, _uow.Transaction) > 0;
    }
}
