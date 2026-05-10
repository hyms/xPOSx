namespace XPos.Domain.Models;

public class Sale
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public bool IsPos { get; set; }
    public long ClientId { get; set; }
    public long WarehouseId { get; set; }
    public double? TaxRate { get; set; }
    public double? TaxNet { get; set; }
    public double? Discount { get; set; }
    public double? Shipping { get; set; }
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public string Status { get; set; } = "completed";
    public string PaymentStatus { get; set; } = "unpaid";
    public string? ShippingStatus { get; set; }
    public string? Notes { get; set; }
    public long? VoucherId { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }

    public List<SaleDetail> Details { get; set; } = new();
    public Voucher? Voucher { get; set; }
}

public class SaleDetail
{
    public long Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public long SaleId { get; set; }
    public long ProductId { get; set; }
    public long? ProductVariantId { get; set; }
    public long? SaleUnitId { get; set; }
    public double Price { get; set; }
    public double? TaxNet { get; set; }
    public string? TaxMethod { get; set; }
    public double? Discount { get; set; }
    public string? DiscountMethod { get; set; }
    public double Total { get; set; }
    public double Quantity { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    
    // Transient property for UI purposes
    public string? ProductName { get; set; }
}

public class SaleReadDto
{
    public long Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public long? VoucherId { get; set; }
}
