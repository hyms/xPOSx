namespace XPos.Domain.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public long? RoleId { get; set; }
    public long? Role { get => RoleId; set => RoleId = value; }
    public bool IsActive { get; set; } = true;
    public bool AllWarehousesAccess { get; set; } = true;
    public long? DefaultWarehouseId { get; set; }
    public List<long> WarehouseIds { get; set; } = new();
    public string? Image { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property for logic
    public Role? RoleDetails { get; set; }
}

public class PasswordUpdateDto
{
    public string NewPassword { get; set; } = string.Empty;
}
