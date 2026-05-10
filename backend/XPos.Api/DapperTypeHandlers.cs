using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is string s && DateTime.TryParse(s, out var d)) return DateOnly.FromDateTime(d);
        if (value is DateOnly do_) return do_;
        throw new InvalidCastException($"Cannot convert {value?.GetType()} ({value}) to DateOnly");
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToString("yyyy-MM-dd");
        if (parameter is NpgsqlParameter np) np.NpgsqlDbType = NpgsqlDbType.Date;
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
