namespace XPos.Domain.Models;

public class Permission
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string GuardName { get; set; } = string.Empty;
}
