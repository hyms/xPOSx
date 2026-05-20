namespace XPos.Domain.Dtos;

public class CreateSaleReturnDto
{
    public DateTime Date { get; set; }
    public long ClientId { get; set; }
    public long WarehouseId { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "completed";
    public string PaymentStatus { get; set; } = "unpaid";
    public List<CreateSaleReturnDetailDto> Details { get; set; } = new();
}

public class CreateSaleReturnDetailDto
{
    public long ProductId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}

public class CreatePurchaseReturnDto
{
    public DateTime Date { get; set; }
    public long ProviderId { get; set; }
    public long WarehouseId { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "completed";
    public string PaymentStatus { get; set; } = "unpaid";
    public List<CreatePurchaseReturnDetailDto> Details { get; set; } = new();
}

public class CreatePurchaseReturnDetailDto
{
    public long ProductId { get; set; }
    public decimal Cost { get; set; }
    public decimal Quantity { get; set; }
}
