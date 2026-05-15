using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        if (value is DateTime dt) return dt;
        if (value is string s && DateTime.TryParse(s, out var d)) return d;
        if (value is DateOnly do_) return do_.ToDateTime(TimeOnly.MinValue);
        throw new InvalidCastException($"Cannot convert {value?.GetType()} ({value}) to DateTime");
    }

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value;
        if (parameter is NpgsqlParameter np) np.NpgsqlDbType = NpgsqlDbType.Timestamp;
    }
}

public class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value)
    {
        if (value is DateTime dt) return new DateTimeOffset(dt, TimeSpan.Zero);
        if (value is DateTimeOffset dto) return dto;
        if (DateTimeOffset.TryParse(value?.ToString(), out var r)) return r;
        throw new InvalidCastException($"Cannot convert {value?.GetType()} ({value}) to DateTimeOffset");
    }

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.Value = value.UtcDateTime;
    }
}
