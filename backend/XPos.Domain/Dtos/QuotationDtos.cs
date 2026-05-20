namespace XPos.Domain.Dtos;

public class CreateQuotationDto
{
    public DateTime Date { get; set; }
    public long ClientId { get; set; }
    public long WarehouseId { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? Discount { get; set; }
    public decimal? Shipping { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    public List<CreateQuotationDetailDto> Details { get; set; } = new();
}

public class CreateQuotationDetailDto
{
    public long ProductId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}

public class UpdateQuotationDto : CreateQuotationDto
{
    public long Id { get; set; }
}
