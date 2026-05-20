using System.Data;
using Moq;
using FluentAssertions;

namespace XPos.Tests;

public class DapperTypeHandlersTests
{
    [Fact]
    public void DateOnlyTypeHandler_Parse_ShouldReturnDateTime()
    {
        var handler = new DateOnlyTypeHandler();
        var dt = new DateTime(2026, 5, 19);
        
        handler.Parse(dt).Should().Be(dt);
        handler.Parse("2026-05-19").Should().Be(dt);
    }

    [Fact]
    public void DateTimeOffsetTypeHandler_Parse_ShouldReturnDateTimeOffset()
    {
        var handler = new DateTimeOffsetTypeHandler();
        var dt = new DateTime(2026, 5, 19);
        var dto = new DateTimeOffset(dt, TimeSpan.Zero);

        handler.Parse(dt).Should().Be(dto);
        handler.Parse(dto).Should().Be(dto);
    }
}
