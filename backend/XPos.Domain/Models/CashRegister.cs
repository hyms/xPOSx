using System;

namespace XPos.Domain.Models;

public class CashRegister
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long WarehouseId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsMatriz { get; set; } = false;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
