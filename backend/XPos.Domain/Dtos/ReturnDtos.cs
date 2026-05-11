namespace XPos.Domain.Dtos;

public class CreateSaleReturnDto
{
    public DateTime Date { get; set; }
    public long ClientId { get; set; }
    public long WarehouseId { get; set; }
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public string Status { get; set; } = "completed";
    public string PaymentStatus { get; set; } = "unpaid";
    public List<CreateSaleReturnDetailDto> Details { get; set; } = new();
}

public class CreateSaleReturnDetailDto
{
    public long ProductId { get; set; }
    public double Price { get; set; }
    public double Quantity { get; set; }
}

public class CreatePurchaseReturnDto
{
    public DateTime Date { get; set; }
    public long ProviderId { get; set; }
    public long WarehouseId { get; set; }
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public string Status { get; set; } = "completed";
    public string PaymentStatus { get; set; } = "unpaid";
    public List<CreatePurchaseReturnDetailDto> Details { get; set; } = new();
}

public class CreatePurchaseReturnDetailDto
{
    public long ProductId { get; set; }
    public double Cost { get; set; }
    public double Quantity { get; set; }
}
