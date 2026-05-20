namespace XPos.Domain.Models;

public class Adjustment
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public long WarehouseId { get; set; }
    public decimal Items { get; set; }
    public string? Notes { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }

    public List<AdjustmentDetail> Details { get; set; } = new();
}

public class AdjustmentDetail
{
    public long Id { get; set; }
    public long AdjustmentId { get; set; }
    public long ProductId { get; set; }
    public long? ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public string Type { get; set; } = "add"; // "add" or "sub"
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}

public class AdjustmentReadDto
{
    public long Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Items { get; set; }
}
