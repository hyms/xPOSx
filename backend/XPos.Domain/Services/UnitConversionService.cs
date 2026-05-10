using XPos.Domain.Models;

namespace XPos.Domain.Services;

/// <summary>
/// Domain service responsible for business logic related to units and inventory conversions.
/// </summary>
public class UnitConversionService
{
    public double CalculateBaseQuantity(double quantity, Unit? unit)
    {
        if (unit == null || string.IsNullOrWhiteSpace(unit.Operator))
            return quantity;

        if (unit.Operator == "*")
            return quantity * unit.OperatorValue;

        if (unit.Operator == "/" && unit.OperatorValue != 0)
            return quantity / unit.OperatorValue;

        return quantity;
    }
}
