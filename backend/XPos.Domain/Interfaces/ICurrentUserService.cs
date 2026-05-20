using System.Security.Claims;

namespace XPos.Domain.Interfaces;

public interface ICurrentUserService
{
    long UserId { get; }
    long? ActiveWarehouseId { get; }
    bool HasAllWarehousesAccess { get; }
    IEnumerable<long> AllowedWarehouseIds { get; }
}