namespace XPos.Domain.Dtos;

public class CreateTransferDto
{
    public DateTime Date { get; set; }
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Discount { get; set; }
    public decimal Shipping { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    public List<CreateTransferDetailDto> Details { get; set; } = new();
}

public class CreateTransferDetailDto
{
    public long ProductId { get; set; }
    public decimal Cost { get; set; }
    public decimal Quantity { get; set; }
}

public class UpdateTransferDto : CreateTransferDto
{
    public long Id { get; set; }
}
