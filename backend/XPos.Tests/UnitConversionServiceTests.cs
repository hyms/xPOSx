using XPos.Domain.Models;
using XPos.Domain.Services;
using FluentAssertions;

namespace XPos.Tests;

public class UnitConversionServiceTests
{
    private readonly UnitConversionService _service;

    public UnitConversionServiceTests()
    {
        _service = new UnitConversionService();
    }

    [Theory]
    [InlineData(10, "*", 2, 20)]
    [InlineData(10, "/", 2, 5)]
    [InlineData(10, "+", 2, 10)] // Invalid operator returns original
    [InlineData(10, null, 2, 10)] // No operator returns original
    public void CalculateBaseQuantity_ShouldPerformCorrectCalculation(decimal quantity, string? op, decimal val, decimal expected)
    {
        // Arrange
        var unit = op == null ? null : new Unit { Operator = op, OperatorValue = val };

        // Act
        var result = _service.CalculateBaseQuantity(quantity, unit);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateBaseQuantity_ShouldReturnQuantity_WhenUnitIsNull()
    {
        // Act
        var result = _service.CalculateBaseQuantity(100, null);

        // Assert
        result.Should().Be(100);
    }
}
