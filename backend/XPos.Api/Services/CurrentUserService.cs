using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using XPos.Domain.Interfaces;

namespace XPos.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirst("id")?.Value;
            return long.TryParse(id, out var result) ? result : 0;
        }
    }

    public long? ActiveWarehouseId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirst("active_warehouse_id")?.Value;
            return long.TryParse(id, out var result) ? result : null;
        }
    }

    public bool HasAllWarehousesAccess
    {
        get
        {
            var val = _httpContextAccessor.HttpContext?.User?.FindFirst("has_all_warehouses_access")?.Value;
            return bool.TryParse(val, out var result) && result;
        }
    }

    public IEnumerable<long> AllowedWarehouseIds
    {
        get
        {
            var val = _httpContextAccessor.HttpContext?.User?.FindFirst("allowed_warehouses")?.Value;
            if (string.IsNullOrEmpty(val)) return Enumerable.Empty<long>();
            return val.Split(',').Select(id => long.TryParse(id, out var r) ? r : 0).Where(r => r > 0);
        }
    }
}