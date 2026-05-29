namespace XPos.Domain.Models;

public class Category
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class Unit
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public long? BaseUnit { get; set; }
    public string Operator { get; set; } = "*";
    public decimal OperatorValue { get; set; } = 1;
}

public class Product
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public long? CategoryId { get; set; }
    public long? UnitId { get; set; }
    public long? UnitSaleId { get; set; }
    public long? UnitPurchaseId { get; set; }
    public decimal TaxNet { get; set; }
    public string TaxMethod { get; set; } = "1";
    public string? Note { get; set; }
    public decimal StockAlert { get; set; }
    public bool IsVariant { get; set; }
    public bool NotSelling { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Image { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsWebAvailable { get; set; } = true;
    
    // Navigation properties (optional for Dapper, but helpful for UI)
    public Category? Category { get; set; }
    public Unit? Unit { get; set; }
    public decimal Stock { get; set; }
}

public class ProductVariant
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string? Name { get; set; }
    public decimal Qty { get; set; }
    public decimal Price { get; set; }
}

public class ProductWarehouse
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long WarehouseId { get; set; }
    public long? ProductVariantId { get; set; }
    public decimal Qty { get; set; }
    public decimal Price { get; set; }
}
