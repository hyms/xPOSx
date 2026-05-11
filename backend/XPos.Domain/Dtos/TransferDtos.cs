namespace XPos.Domain.Dtos;

public class CreateTransferDto
{
    public DateTime Date { get; set; }
    public long FromWarehouseId { get; set; }
    public long ToWarehouseId { get; set; }
    public double TaxRate { get; set; }
    public double Discount { get; set; }
    public double Shipping { get; set; }
    public string Status { get; set; } = "Completed";
    public string? Notes { get; set; }
    public List<CreateTransferDetailDto> Details { get; set; } = new();
}

public class CreateTransferDetailDto
{
    public long ProductId { get; set; }
    public double Cost { get; set; }
    public double Quantity { get; set; }
}

public class UpdateTransferDto : CreateTransferDto
{
    public long Id { get; set; }
}
