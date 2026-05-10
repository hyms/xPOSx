namespace XPos.Domain.Models;

public class Role
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GuardName { get; set; } = string.Empty;
    public List<Permission> Permissions { get; set; } = new();
}
