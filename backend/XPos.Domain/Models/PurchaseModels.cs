namespace XPos.Domain.Models;

public class Purchase
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public long ProviderId { get; set; }
    public long WarehouseId { get; set; }
    public double? TaxRate { get; set; }
    public double? TaxNet { get; set; }
    public double? Discount { get; set; }
    public double? Shipping { get; set; }
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public string Status { get; set; } = "received";
    public string PaymentStatus { get; set; } = "unpaid";
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }

    public long? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
    public List<PurchaseDetail> Details { get; set; } = new();
}

public class PurchaseDetail
{
    public long Id { get; set; }
    public long PurchaseId { get; set; }
    public long ProductId { get; set; }
    public long? ProductVariantId { get; set; }
    public long? PurchaseUnitId { get; set; }
    public double Cost { get; set; }
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
}

public class PurchaseReadDto
{
    public long Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public double DueAmount => GrandTotal - PaidAmount;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public long? VoucherId { get; set; }
}
