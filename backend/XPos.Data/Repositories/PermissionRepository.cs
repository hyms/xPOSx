using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly string _connectionString;

    public PermissionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM permissions";
        return await connection.QueryAsync<Permission>(sql);
    }

    public async Task<Permission?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM permissions WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Permission>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Permission>> GetByRoleIdAsync(long roleId)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT p.* FROM permissions p
            INNER JOIN role_has_permissions rp ON p.id = rp.permission_id
            WHERE rp.role_id = @RoleId";
        return await connection.QueryAsync<Permission>(sql, new { RoleId = roleId });
    }
}
