namespace XPos.Domain.Models;

public class ExpenseCategory
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }
}

public class Expense
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public long? UserId { get; set; }
    public long ExpenseCategoryId { get; set; }
    public long WarehouseId { get; set; }
    public string Details { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    
    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }

    // Navigation (Optional for DTO mapping)
    public string? CategoryName { get; set; }
    public string? WarehouseName { get; set; }
}

public class ExpenseReadDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
