using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly IUnitOfWork _uow;

    public PermissionRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        const string sql = "SELECT * FROM permissions";
        return await _uow.Connection.QueryAsync<Permission>(sql, null, _uow.Transaction);
    }

    public async Task<Permission?> GetByIdAsync(long id)
    {
        const string sql = "SELECT * FROM permissions WHERE id = @Id";
        return await _uow.Connection.QueryFirstOrDefaultAsync<Permission>(sql, new { Id = id }, _uow.Transaction);
    }

    public async Task<IEnumerable<Permission>> GetByRoleIdAsync(long roleId)
    {
        const string sql = @"
            SELECT p.* FROM permissions p
            INNER JOIN role_has_permissions rp ON p.id = rp.permission_id
            WHERE rp.role_id = @RoleId";
        return await _uow.Connection.QueryAsync<Permission>(sql, new { RoleId = roleId }, _uow.Transaction);
    }
}
