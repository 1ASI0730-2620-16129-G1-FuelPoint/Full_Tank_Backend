namespace TechnoSac.FullTank.Platform.Shared.Domain.Services;

/// <summary>
/// Domain service for authoritative fuel unit conversions between Liters and Gallons.
/// Supports both short ('L', 'LT', 'GAL', 'GL') and long ('LITER', 'LITERS', 'GALLON', 'GALLONS') unit codes.
/// Persistence uses integer quantities; conversions use conservative ceiling for inventory stock reservations
/// and banker's rounding for equipment refills with overflow protection.
/// </summary>
public static class FuelUnitConverter
{
    public const decimal LitersPerGallon = 3.78541m;

    public static bool IsGallon(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return false;
        var trimmed = unit.Trim();
        return string.Equals(trimmed, "GAL", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(trimmed, "GL", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(trimmed, "GALLON", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(trimmed, "GALLONS", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsLiter(string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return false;
        var trimmed = unit.Trim();
        return string.Equals(trimmed, "L", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(trimmed, "LT", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(trimmed, "LITER", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(trimmed, "LITERS", StringComparison.OrdinalIgnoreCase);
    }

    public static decimal ConvertToLiters(decimal quantity, string unit)
    {
        if (IsGallon(unit))
            return quantity * LitersPerGallon;
        return quantity;
    }

    public static decimal ConvertQuantity(decimal quantity, string fromUnit, string toUnit)
    {
        if (string.IsNullOrWhiteSpace(fromUnit) || string.IsNullOrWhiteSpace(toUnit))
            return quantity;
        if (string.Equals(fromUnit.Trim(), toUnit.Trim(), StringComparison.OrdinalIgnoreCase))
            return quantity;
        if (IsGallon(fromUnit) && IsLiter(toUnit))
            return quantity * LitersPerGallon;
        if (IsLiter(fromUnit) && IsGallon(toUnit))
            return quantity / LitersPerGallon;
        return quantity;
    }

    /// <summary>
    /// Converts a requested order quantity into inventory stock units using a conservative ceiling rule.
    /// Ensures that providers never under-reserve stock due to fractional gallon-to-liter conversions.
    /// Rejects negative, zero, or overflow values.
    /// </summary>
    public static int ConvertToStockUnitsCeil(decimal quantity, string fromUnit, string targetStockUnit)
    {
        if (quantity <= 0) return 0;
        var converted = ConvertQuantity(quantity, fromUnit, targetStockUnit);
        var ceiled = Math.Ceiling(converted);
        if (ceiled > int.MaxValue)
            throw new OverflowException($"Quantity {ceiled} exceeds maximum 32-bit integer capacity.");
        return (int)ceiled;
    }

    /// <summary>
    /// Converts order fuel quantity into target equipment capacity units.
    /// Rejects non-positive or overflow quantities.
    /// </summary>
    public static int ConvertToEquipmentUnits(decimal quantity, string fromUnit, string targetEquipmentUnit)
    {
        if (quantity <= 0) return 0;
        var converted = ConvertQuantity(quantity, fromUnit, targetEquipmentUnit);
        var rounded = Math.Round(converted, MidpointRounding.AwayFromZero);
        if (rounded > int.MaxValue)
            throw new OverflowException($"Refill quantity {rounded} exceeds maximum 32-bit integer capacity.");
        return (int)rounded;
    }

    public static decimal CalculateUnitPrice(decimal pricePerLiter, string targetUnit)
    {
        if (IsGallon(targetUnit))
        {
            return Math.Round(pricePerLiter * LitersPerGallon, 2, MidpointRounding.AwayFromZero);
        }

        return Math.Round(pricePerLiter, 2, MidpointRounding.AwayFromZero);
    }
}
