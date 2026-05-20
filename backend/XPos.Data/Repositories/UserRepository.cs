using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using BC = BCrypt.Net.BCrypt;

namespace XPos.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT 
                u.id AS Id, 
                u.username AS Username, 
                u.email AS Email, 
                u.first_name AS FirstName, 
                u.last_name AS LastName, 
                u.is_active AS IsActive,
                r.id AS Id, 
                r.name AS Name 
            FROM users u
            LEFT JOIN roles r ON u.role_id = r.id";
        
        return await connection.QueryAsync<User, Role, User>(
            sql,
            (user, role) => {
                user.RoleDetails = role;
                return user;
            },
            splitOn: "Id"
        );
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM users WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM users WHERE username = @Username";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<long> CreateAsync(User user)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO users (username, password, email, first_name, last_name, role_id, is_active, default_warehouse_id, created_at)
            VALUES (@Username, @Password, @Email, @FirstName, @LastName, @RoleId, @IsActive, @DefaultWarehouseId, @CreatedAt)
            RETURNING id";
        
        user.Password = BC.HashPassword(user.Password);
        user.CreatedAt = DateTime.UtcNow;
        return await connection.ExecuteScalarAsync<long>(sql, user);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE users 
            SET username = @Username, 
                email = @Email, 
                first_name = @FirstName, 
                last_name = @LastName, 
                role_id = @RoleId, 
                is_active = @IsActive, 
                default_warehouse_id = @DefaultWarehouseId,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        user.UpdatedAt = DateTime.UtcNow;
        var rowsAffected = await connection.ExecuteAsync(sql, user);
        return rowsAffected > 0;
    }

    public async Task<bool> UpdatePasswordAsync(long id, string newPassword)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE users SET password = @Password, updated_at = @UpdatedAt WHERE id = @Id";
        var hashedPassword = BC.HashPassword(newPassword);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Password = hashedPassword, UpdatedAt = DateTime.UtcNow, Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "DELETE FROM users WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ToggleUserStatusAsync(long id)
    {
        using var connection = CreateConnection();
        const string sql = "UPDATE users SET is_active = NOT is_active, updated_at = @UpdatedAt WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { UpdatedAt = DateTime.UtcNow, Id = id });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<long>> GetUserWarehouseIdsAsync(long userId)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT warehouse_id FROM user_warehouse WHERE user_id = @UserId";
        return await connection.QueryAsync<long>(sql, new { UserId = userId });
    }
}
