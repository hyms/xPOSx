using Dapper;
using XPos.Domain.Interfaces;

namespace XPos.Data.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public InventoryRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task UpdateStockAsync(long productId, long warehouseId, decimal quantityChange)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        // Enforce warehouse isolation for stock updates
        if (!hasAllAccess && warehouseId != activeWarehouseId)
        {
            throw new UnauthorizedAccessException("User does not have permission to update stock in this warehouse.");
        }

        Console.WriteLine($"UpdateStockAsync: ProductId={productId}, WarehouseId={warehouseId}, QtyChange={quantityChange}");
        var sql = "UPDATE product_warehouse SET qty = qty + @QuantityChange WHERE product_id = @ProductId AND warehouse_id = @WarehouseId";
        var rowsAffected = await _uow.Connection.ExecuteAsync(sql, new { QuantityChange = quantityChange, ProductId = productId, WarehouseId = warehouseId }, _uow.Transaction);
        
        Console.WriteLine($"UpdateStockAsync: RowsAffected={rowsAffected}");
        if (rowsAffected == 0 && quantityChange > 0)
        {
            // If the record doesn't exist and we are adding stock, create it.
            var insertSql = "INSERT INTO product_warehouse (product_id, warehouse_id, qty, created_at) VALUES (@ProductId, @WarehouseId, @QuantityChange, CURRENT_TIMESTAMP)";
            var inserted = await _uow.Connection.ExecuteAsync(insertSql, new { ProductId = productId, WarehouseId = warehouseId, QuantityChange = quantityChange }, _uow.Transaction);
            Console.WriteLine($"UpdateStockAsync: Inserted={inserted}");
        }
    }
}
