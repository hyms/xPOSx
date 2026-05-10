namespace XPos.Domain.Models;

public class Client
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public int? Code { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string NitCi { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
