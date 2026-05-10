using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class SettingRepository : ISettingRepository
{
    private readonly string _connectionString;

    public SettingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<Setting?> GetAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM settings ORDER BY id LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<Setting>(sql);
    }

    public async Task<bool> UpdateAsync(Setting setting)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE settings 
            SET company_name = @CompanyName, 
                email = @Email, 
                company_phone = @CompanyPhone, 
                company_address = @CompanyAddress, 
                logo = @Logo, 
                version = @Version, 
                days = @Days, 
                updated_at = @UpdatedAt
            WHERE id = @Id";
        setting.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, setting);
        return rowsAffected > 0;
    }
}
