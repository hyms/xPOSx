namespace XPos.Domain.Models;

public class SaleReturn
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public long? SaleId { get; set; }
    public long ClientId { get; set; }
    public long WarehouseId { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxNet { get; set; }
    public decimal? Discount { get; set; }
    public decimal? Shipping { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "completed";
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
    public List<SaleReturnDetail> Details { get; set; } = new();
}

public class SaleReturnDetail
{
    public long Id { get; set; }
    public long SaleReturnId { get; set; }
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
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ProductName { get; set; }
}

public class PurchaseReturn
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public long? PurchaseId { get; set; }
    public long ProviderId { get; set; }
    public long WarehouseId { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxNet { get; set; }
    public decimal? Discount { get; set; }
    public decimal? Shipping { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "completed";
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
    public List<PurchaseReturnDetail> Details { get; set; } = new();
}

public class PurchaseReturnDetail
{
    public long Id { get; set; }
    public long PurchaseReturnId { get; set; }
    public long ProductId { get; set; }
    public long? ProductVariantId { get; set; }
    public long? PurchaseUnitId { get; set; }
    public decimal Cost { get; set; }
    public decimal? TaxNet { get; set; }
    public string? TaxMethod { get; set; }
    public decimal? Discount { get; set; }
    public string? DiscountMethod { get; set; }
    public decimal Total { get; set; }
    public decimal Quantity { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ProductName { get; set; }
}

public class SaleReturnReadDto
{
    public long Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public long? VoucherId { get; set; }
}

public class PurchaseReturnReadDto
{
    public long Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public long? VoucherId { get; set; }
}
