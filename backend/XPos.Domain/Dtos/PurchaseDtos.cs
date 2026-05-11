namespace XPos.Domain.Dtos;

public class CreatePurchaseDto
{
    public DateTime Date { get; set; }
    public long ProviderId { get; set; }
    public long WarehouseId { get; set; }
    public double TaxRate { get; set; }
    public double Discount { get; set; }
    public double Shipping { get; set; }
    public string Status { get; set; } = "received";
    public string PaymentStatus { get; set; } = "unpaid";
    public string? Notes { get; set; }
    public List<CreatePurchaseDetailDto> Details { get; set; } = new();
    public CreateVoucherDto? Voucher { get; set; }
}

public class CreatePurchaseDetailDto
{
    public long ProductId { get; set; }
    public double Cost { get; set; }
    public double Quantity { get; set; }
    public long? PurchaseUnitId { get; set; }
}

public class CreateVoucherDto
{
    public string VoucherType { get; set; } = string.Empty;
    public string VoucherNumber { get; set; } = string.Empty;
    public string Cae { get; set; } = string.Empty;
    public DateTime CaeExpiration { get; set; }
}

public class UpdatePurchaseDto : CreatePurchaseDto
{
    public long Id { get; set; }
}
