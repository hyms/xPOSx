using Dapper.Contrib.Extensions;

namespace XPos.Domain.Models;

[Table("CurrencySettings")]
public class CurrencySetting
{
    [Key]
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}
