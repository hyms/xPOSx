namespace XPos.Domain.Models;

public class Transfer
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public double Items { get; set; }
    public double TaxRate { get; set; }
    public double TaxNet { get; set; }
    public double Discount { get; set; }
    public double Shipping { get; set; }
    public double GrandTotal { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    
    // Audit
    public DateTime? CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public List<TransferDetail> Details { get; set; } = new();
}

public class TransferDetail
{
    public long Id { get; set; }
    public long TransferId { get; set; }
    public long ProductId { get; set; }
    public long? ProductVariantId { get; set; }
    public long? PurchaseUnitId { get; set; }
    public double Cost { get; set; }
    public double TaxNet { get; set; }
    public string TaxMethod { get; set; } = "1";
    public double Discount { get; set; }
    public string DiscountMethod { get; set; } = "1";
    public double Quantity { get; set; }
    public double Total { get; set; }
}

public class TransferReadDto
{
    public long Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;
    public string ToWarehouseName { get; set; } = string.Empty;
    public double Items { get; set; }
    public double GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}
