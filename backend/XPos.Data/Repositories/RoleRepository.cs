using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly string _connectionString;

    public RoleRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM roles";
        return await connection.QueryAsync<Role>(sql);
    }

    public async Task<Role?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT r.id, r.name, r.guard_name, p.id, p.name, p.guard_name
            FROM roles r
            LEFT JOIN role_has_permissions rp ON r.id = rp.role_id
            LEFT JOIN permissions p ON rp.permission_id = p.id
            WHERE r.id = @Id";

        var roleDictionary = new Dictionary<long, Role>();

        var result = await connection.QueryAsync<Role, Permission, Role>(
            sql,
            (role, permission) =>
            {
                if (!roleDictionary.TryGetValue(role.Id, out var roleEntry))
                {
                    roleEntry = role;
                    roleDictionary.Add(roleEntry.Id, roleEntry);
                }

                if (permission != null && permission.Id > 0)
                {
                    roleEntry.Permissions.Add(permission);
                }

                return roleEntry;
            },
            new { Id = id },
            splitOn: "id"
        );

        return roleDictionary.Values.FirstOrDefault();
    }

    public async Task<long> CreateAsync(Role role)
    {
        using var connection = CreateConnection();
        const string sql = "INSERT INTO roles (name, guard_name) VALUES (@Name, @GuardName) RETURNING id";
        return await connection.ExecuteScalarAsync<long>(sql, role);
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE roles SET name = @Name, guard_name = @GuardName WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, role);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = CreateConnection();
        // Should also handle cleaning up role_has_permissions
        const string deleteRelSql = "DELETE FROM role_has_permissions WHERE role_id = @Id";
        await connection.ExecuteAsync(deleteRelSql, new { Id = id });
        
        const string sql = "DELETE FROM roles WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> AssignPermissionsAsync(long roleId, IEnumerable<long> permissionIds)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string deleteSql = "DELETE FROM role_has_permissions WHERE role_id = @RoleId";
            await connection.ExecuteAsync(deleteSql, new { RoleId = roleId }, transaction);

            const string insertSql = "INSERT INTO role_has_permissions (role_id, permission_id) VALUES (@RoleId, @PermissionId)";
            foreach (var permissionId in permissionIds)
            {
                await connection.ExecuteAsync(insertSql, new { RoleId = roleId, PermissionId = permissionId }, transaction);
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}
