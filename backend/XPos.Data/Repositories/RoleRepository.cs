using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IUnitOfWork _uow;

    public RoleRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        const string sql = "SELECT * FROM roles";
        return await _uow.Connection.QueryAsync<Role>(sql, null, _uow.Transaction);
    }

    public async Task<Role?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT r.id, r.name, r.guard_name, p.id, p.name, p.guard_name
            FROM roles r
            LEFT JOIN role_has_permissions rp ON r.id = rp.role_id
            LEFT JOIN permissions p ON rp.permission_id = p.id
            WHERE r.id = @Id";

        var roleDictionary = new Dictionary<long, Role>();

        var result = await _uow.Connection.QueryAsync<Role, Permission, Role>(
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
            splitOn: "id",
            transaction: _uow.Transaction
        );

        return roleDictionary.Values.FirstOrDefault();
    }

    public async Task<long> CreateAsync(Role role)
    {
        const string sql = "INSERT INTO roles (name, guard_name) VALUES (@Name, @GuardName) RETURNING id";
        return await _uow.Connection.ExecuteScalarAsync<long>(sql, role, _uow.Transaction);
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        const string sql = "UPDATE roles SET name = @Name, guard_name = @GuardName WHERE id = @Id";
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, role, _uow.Transaction);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string deleteRelSql = "DELETE FROM role_has_permissions WHERE role_id = @Id";
        await _uow.Connection.ExecuteAsync(deleteRelSql, new { Id = id }, _uow.Transaction);
        
        const string sql = "DELETE FROM roles WHERE id = @Id";
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction);
        return rowsAffected > 0;
    }

    public async Task<bool> AssignPermissionsAsync(long roleId, IEnumerable<long> permissionIds)
    {
        const string deleteSql = "DELETE FROM role_has_permissions WHERE role_id = @RoleId";
        await _uow.Connection.ExecuteAsync(deleteSql, new { RoleId = roleId }, _uow.Transaction);

        const string insertSql = "INSERT INTO role_has_permissions (role_id, permission_id) VALUES (@RoleId, @PermissionId)";
        foreach (var permissionId in permissionIds)
        {
            await _uow.Connection.ExecuteAsync(insertSql, new { RoleId = roleId, PermissionId = permissionId }, _uow.Transaction);
        }

        return true;
    }
}
