namespace XPos.Domain.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class PagingParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "date";
    public bool SortDescending { get; set; } = true;
    public string? Filter { get; set; }
    public long? WarehouseId { get; set; }
}
