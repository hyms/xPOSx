using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;

namespace XPos.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private readonly string _connectionString;
    private readonly IAuditContextProvider _auditContextProvider;

    public UnitOfWork(IConfiguration configuration, IAuditContextProvider auditContextProvider)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();
        _auditContextProvider = auditContextProvider;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
                SetAuditSessionVariables(_connection);
            }
            return _connection;
        }
    }

    private void SetAuditSessionVariables(IDbConnection connection)
    {
        try
        {
            var userId = _auditContextProvider.GetUserId()?.ToString() ?? "";
            var ipAddress = _auditContextProvider.GetIpAddress();
            var userAgent = _auditContextProvider.GetUserAgent();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT set_config('audit.user_id', @UserId, false),
                       set_config('audit.ip_address', @IpAddress, false),
                       set_config('audit.user_agent', @UserAgent, false);";

            var pUserId = command.CreateParameter();
            pUserId.ParameterName = "UserId";
            pUserId.Value = userId;
            command.Parameters.Add(pUserId);

            var pIpAddress = command.CreateParameter();
            pIpAddress.ParameterName = "IpAddress";
            pIpAddress.Value = ipAddress;
            command.Parameters.Add(pIpAddress);

            var pUserAgent = command.CreateParameter();
            pUserAgent.ParameterName = "UserAgent";
            pUserAgent.Value = userAgent;
            command.Parameters.Add(pUserAgent);

            command.ExecuteNonQuery();
        }
        catch
        {
            // Fail-safe to ensure database connection still works if auditing has an issue
        }
    }

    public IDbTransaction? Transaction => _transaction;

    public void BeginTransaction()
    {
        _transaction = Connection.BeginTransaction();
    }

    public void Commit()
    {
        try
        {
            _transaction?.Commit();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
    }
}
