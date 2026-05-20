namespace XPos.Domain.Models;

public class Quotation
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public long ClientId { get; set; }
    public long WarehouseId { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxNet { get; set; }
    public decimal? Discount { get; set; }
    public decimal? Shipping { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }

    public List<QuotationDetail> Details { get; set; } = new();
}

public class QuotationDetail
{
    public long Id { get; set; }
    public long QuotationId { get; set; }
    public long ProductId { get; set; }
    public long? ProductVariantId { get; set; }
    public long? SaleUnitId { get; set; }
    public decimal Price { get; set; }
    public decimal? TaxNet { get; set; }
    public string? TaxMethod { get; set; }
    public decimal? Discount { get; set; }
    public string? DiscountMethod { get; set; }
    public decimal Total { get; set; }
    public decimal Quantity { get; set; }
}

public class QuotationReadDto
{
    public long Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}
