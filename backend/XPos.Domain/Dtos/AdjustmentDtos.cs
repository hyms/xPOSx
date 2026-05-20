namespace XPos.Domain.Dtos;

public class CreateAdjustmentDto
{
    public DateTime Date { get; set; }
    public long WarehouseId { get; set; }
    public string? Notes { get; set; }
    public List<CreateAdjustmentDetailDto> Details { get; set; } = new();
}

public class CreateAdjustmentDetailDto
{
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string Type { get; set; } = "add";
}

public class UpdateAdjustmentDto : CreateAdjustmentDto
{
    public long Id { get; set; }
}
