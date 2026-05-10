namespace XPos.Domain.Models;

public class Warehouse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
