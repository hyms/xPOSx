using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using BC = BCrypt.Net.BCrypt;

namespace XPos.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IUnitOfWork _uow;

    public UserRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = @"
            SELECT 
                u.id AS Id, 
                u.username AS Username, 
                u.email AS Email, 
                u.first_name AS FirstName, 
                u.last_name AS LastName, 
                u.is_active AS IsActive,
                u.all_warehouses_access AS AllWarehousesAccess,
                u.role_id AS RoleId,
                u.default_warehouse_id AS DefaultWarehouseId,
                r.id AS Id, 
                r.name AS Name 
            FROM users u
            LEFT JOIN roles r ON u.role_id = r.id";
        
        var users = await _uow.Connection.QueryAsync<User, Role, User>(
            sql,
            (user, role) => {
                user.RoleDetails = role;
                return user;
            },
            splitOn: "Id",
            transaction: _uow.Transaction
        );

        foreach (var user in users)
        {
            user.WarehouseIds = (await GetUserWarehouseIdsAsync(user.Id)).ToList();
        }

        return users;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        const string sql = "SELECT id, username, password, email, first_name AS FirstName, last_name AS LastName, role_id AS RoleId, is_active AS IsActive, all_warehouses_access AS AllWarehousesAccess, default_warehouse_id AS DefaultWarehouseId, created_at AS CreatedAt, updated_at AS UpdatedAt FROM users WHERE id = @Id";
        var user = await _uow.Connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id }, _uow.Transaction);
        if (user != null)
        {
            user.WarehouseIds = (await GetUserWarehouseIdsAsync(user.Id)).ToList();
        }
        return user;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = "SELECT id, username, password, email, first_name AS FirstName, last_name AS LastName, role_id AS RoleId, is_active AS IsActive, all_warehouses_access AS AllWarehousesAccess, default_warehouse_id AS DefaultWarehouseId, created_at AS CreatedAt, updated_at AS UpdatedAt FROM users WHERE username = @Username";
        var user = await _uow.Connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username }, _uow.Transaction);
        if (user != null)
        {
            user.WarehouseIds = (await GetUserWarehouseIdsAsync(user.Id)).ToList();
        }
        return user;
    }

    public async Task<long> CreateAsync(User user)
    {
        const string sql = @"
            INSERT INTO users (username, password, email, first_name, last_name, role_id, is_active, all_warehouses_access, default_warehouse_id, created_at)
            VALUES (@Username, @Password, @Email, @FirstName, @LastName, @RoleId, @IsActive, @AllWarehousesAccess, @DefaultWarehouseId, @CreatedAt)
            RETURNING id";
        
        user.Password = BC.HashPassword(user.Password);
        user.CreatedAt = DateTime.UtcNow;
        var id = await _uow.Connection.ExecuteScalarAsync<long>(sql, user, _uow.Transaction);
        user.Id = id;

        if (user.WarehouseIds != null && user.WarehouseIds.Any())
        {
            const string insertWarehouseSql = "INSERT INTO user_warehouse (user_id, warehouse_id) VALUES (@UserId, @WarehouseId)";
            foreach (var whId in user.WarehouseIds)
            {
                await _uow.Connection.ExecuteAsync(insertWarehouseSql, new { UserId = id, WarehouseId = whId }, _uow.Transaction);
            }
        }

        return id;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        const string sql = @"
            UPDATE users 
            SET username = @Username, 
                email = @Email, 
                first_name = @FirstName, 
                last_name = @LastName, 
                role_id = @RoleId, 
                is_active = @IsActive, 
                all_warehouses_access = @AllWarehousesAccess,
                default_warehouse_id = @DefaultWarehouseId,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        user.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, user, _uow.Transaction);

        if (rowsAffected > 0)
        {
            const string deleteWarehouseSql = "DELETE FROM user_warehouse WHERE user_id = @UserId";
            await _uow.Connection.ExecuteAsync(deleteWarehouseSql, new { UserId = user.Id }, _uow.Transaction);

            if (user.WarehouseIds != null && user.WarehouseIds.Any())
            {
                const string insertWarehouseSql = "INSERT INTO user_warehouse (user_id, warehouse_id) VALUES (@UserId, @WarehouseId)";
                foreach (var whId in user.WarehouseIds)
                {
                    await _uow.Connection.ExecuteAsync(insertWarehouseSql, new { UserId = user.Id, WarehouseId = whId }, _uow.Transaction);
                }
            }
        }

        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(long id, string newPassword)
    {
        const string sql = "UPDATE users SET password = @Password, updated_at = @UpdatedAt WHERE id = @Id";
        var hashedPassword = BC.HashPassword(newPassword);
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { Password = hashedPassword, UpdatedAt = DateTime.UtcNow, Id = id }, _uow.Transaction);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM users WHERE id = @Id";
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { Id = id }, _uow.Transaction);
        return rowsAffected > 0;
    }

    public async Task<bool> ToggleUserStatusAsync(long id)
    {
        const string sql = "UPDATE users SET is_active = NOT is_active, updated_at = @UpdatedAt WHERE id = @Id";
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { UpdatedAt = DateTime.UtcNow, Id = id }, _uow.Transaction);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<long>> GetUserWarehouseIdsAsync(long userId)
    {
        const string sql = "SELECT warehouse_id FROM user_warehouse WHERE user_id = @UserId";
        return await _uow.Connection.QueryAsync<long>(sql, new { UserId = userId }, _uow.Transaction);
    }
}
