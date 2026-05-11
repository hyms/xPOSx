namespace XPos.Domain.Dtos;

public class CreateQuotationDto
{
    public DateTime Date { get; set; }
    public long ClientId { get; set; }
    public long WarehouseId { get; set; }
    public double? TaxRate { get; set; }
    public double? Discount { get; set; }
    public double? Shipping { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    public List<CreateQuotationDetailDto> Details { get; set; } = new();
}

public class CreateQuotationDetailDto
{
    public long ProductId { get; set; }
    public double Price { get; set; }
    public double Quantity { get; set; }
}

public class UpdateQuotationDto : CreateQuotationDto
{
    public long Id { get; set; }
}
