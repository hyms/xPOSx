using Dapper;
using System.Data;
using System.Text;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public ReportRepository(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            SELECT s.date::timestamp as ""Date"", s.ref as ""Ref"", c.name as ""ClientName"", w.name as ""WarehouseName"", s.grand_total as ""GrandTotal"", s.paid_amount as ""PaidAmount"", s.payment_status as ""PaymentStatus""
            FROM sales s
            JOIN clients c ON s.client_id = c.id
            JOIN warehouses w ON s.warehouse_id = w.id
            WHERE s.deleted_at IS NULL AND c.deleted_at IS NULL AND w.deleted_at IS NULL";

        var parameters = new DynamicParameters();
        
        if (!hasAllAccess)
        {
            sql += " AND s.warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }
        else if (warehouseId.HasValue)
        {
            sql += " AND s.warehouse_id = @warehouseId";
            parameters.Add("warehouseId", warehouseId.Value);
        }

        if (startDate.HasValue) { sql += " AND s.date >= @startDate"; parameters.Add("startDate", startDate.Value); }
        if (endDate.HasValue) { sql += " AND s.date <= @endDate"; parameters.Add("endDate", endDate.Value); }

        sql += " ORDER BY s.date DESC";
        return await _uow.Connection.QueryAsync<SalesReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<PurchaseReportDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            SELECT p.date::timestamp as ""Date"", p.ref as ""Ref"", pr.name as ""ProviderName"", w.name as ""WarehouseName"", p.grand_total as ""GrandTotal"", p.paid_amount as ""PaidAmount"", p.status as ""Status""
            FROM purchases p
            JOIN providers pr ON p.provider_id = pr.id
            JOIN warehouses w ON p.warehouse_id = w.id
            WHERE p.deleted_at IS NULL AND pr.deleted_at IS NULL AND w.deleted_at IS NULL";

        var parameters = new DynamicParameters();

        if (!hasAllAccess)
        {
            sql += " AND p.warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }
        else if (warehouseId.HasValue)
        {
            sql += " AND p.warehouse_id = @warehouseId";
            parameters.Add("warehouseId", warehouseId.Value);
        }

        if (startDate.HasValue) { sql += " AND p.date >= @startDate"; parameters.Add("startDate", startDate.Value); }
        if (endDate.HasValue) { sql += " AND p.date <= @endDate"; parameters.Add("endDate", endDate.Value); }

        sql += " ORDER BY p.date DESC";
        return await _uow.Connection.QueryAsync<PurchaseReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<StockReportDto>> GetStockReportAsync(long? warehouseId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        string sql = @"
            SELECT p.code as ProductCode, p.name as ProductName, cat.name as CategoryName, w.name as WarehouseName, pw.qty as Quantity, p.stock_alert as StockAlert
            FROM product_warehouse pw
            JOIN products p ON pw.product_id = p.id
            JOIN warehouses w ON pw.warehouse_id = w.id
            JOIN categories cat ON p.category_id = cat.id
            WHERE p.deleted_at IS NULL AND w.deleted_at IS NULL AND cat.deleted_at IS NULL";

        var parameters = new DynamicParameters();

        if (!hasAllAccess)
        {
            sql += " AND pw.warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }
        else if (warehouseId.HasValue)
        {
            sql += " AND pw.warehouse_id = @warehouseId";
            parameters.Add("warehouseId", warehouseId.Value);
        }

        sql += " ORDER BY pw.qty ASC";
        return await _uow.Connection.QueryAsync<StockReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var parameters = new DynamicParameters();
        string dateFilter = "";
        if (startDate.HasValue) { dateFilter += " AND date >= @startDate"; parameters.Add("startDate", startDate.Value); }
        if (endDate.HasValue) { dateFilter += " AND date <= @endDate"; parameters.Add("endDate", endDate.Value); }

        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        var salesSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE deleted_at IS NULL {dateFilter} {warehouseFilter}";
        var purchasesSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM purchases WHERE deleted_at IS NULL {dateFilter} {warehouseFilter}";
        var expensesSql = $"SELECT COALESCE(SUM(amount), 0) FROM expenses WHERE deleted_at IS NULL {dateFilter} {warehouseFilter}";
        var returnsSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM sale_returns WHERE deleted_at IS NULL {dateFilter} {warehouseFilter}";

        var totalSales = await _uow.Connection.ExecuteScalarAsync<decimal>(salesSql, parameters, _uow.Transaction);
        var totalPurchases = await _uow.Connection.ExecuteScalarAsync<decimal>(purchasesSql, parameters, _uow.Transaction);
        var totalExpenses = await _uow.Connection.ExecuteScalarAsync<decimal>(expensesSql, parameters, _uow.Transaction);
        var totalReturns = await _uow.Connection.ExecuteScalarAsync<decimal>(returnsSql, parameters, _uow.Transaction);

        return new ProfitLossReportDto
        {
            TotalSales = totalSales,
            TotalPurchases = totalPurchases,
            TotalExpenses = totalExpenses,
            TotalReturns = totalReturns
        };
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var parameters = new DynamicParameters();
        parameters.Add("today", today);
        parameters.Add("firstDayOfMonth", firstDayOfMonth);

        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        var todaySalesSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE date = @today AND deleted_at IS NULL {warehouseFilter}";
        var todaySalesCountSql = $"SELECT COUNT(*) FROM sales WHERE date = @today AND deleted_at IS NULL {warehouseFilter}";
        var monthlySalesSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE date >= @firstDayOfMonth AND deleted_at IS NULL {warehouseFilter}";
        var monthlyPurchasesSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM purchases WHERE date >= @firstDayOfMonth AND deleted_at IS NULL {warehouseFilter}";
        
        var recentSalesSql = $@"
            SELECT s.ref, c.name as ClientName, s.grand_total as GrandTotal, s.status
            FROM sales s
            JOIN clients c ON s.client_id = c.id
            WHERE s.deleted_at IS NULL {warehouseFilter.Replace("warehouse_id", "s.warehouse_id")}
            ORDER BY s.created_at DESC
            LIMIT 5";

        var topProductsSql = $@"
            SELECT p.name, SUM(sd.quantity) as Quantity, SUM(sd.total) as Total
            FROM sale_details sd
            JOIN products p ON sd.product_id = p.id
            JOIN sales s ON sd.sale_id = s.id
            WHERE s.deleted_at IS NULL {warehouseFilter.Replace("warehouse_id", "s.warehouse_id")}
            GROUP BY p.name
            ORDER BY Total DESC
            LIMIT 5";

        var summary = new DashboardSummaryDto();
        summary.TodaySales = await _uow.Connection.ExecuteScalarAsync<decimal>(todaySalesSql, parameters, _uow.Transaction);
        summary.TodaySalesCount = await _uow.Connection.ExecuteScalarAsync<int>(todaySalesCountSql, parameters, _uow.Transaction);
        summary.MonthlySales = await _uow.Connection.ExecuteScalarAsync<decimal>(monthlySalesSql, parameters, _uow.Transaction);
        summary.MonthlyPurchases = await _uow.Connection.ExecuteScalarAsync<decimal>(monthlyPurchasesSql, parameters, _uow.Transaction);
        summary.RecentSales = (await _uow.Connection.QueryAsync<RecentSaleDto>(recentSalesSql, parameters, _uow.Transaction)).ToList();
        summary.TopProducts = (await _uow.Connection.QueryAsync<TopProductDto>(topProductsSql, parameters, _uow.Transaction)).ToList();

        return summary;
    }

    public async Task<IEnumerable<ClientReportDto>> GetClientReportAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var parameters = new DynamicParameters();
        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        string sql = $@"
            SELECT 
                c.id as ClientId,
                c.name as ClientName,
                c.phone as Phone,
                c.email as Email,
                (SELECT COUNT(*) FROM sales WHERE client_id = c.id AND deleted_at IS NULL {warehouseFilter}) as TotalSales,
                (SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE client_id = c.id AND deleted_at IS NULL {warehouseFilter}) as TotalAmount,
                (SELECT COALESCE(SUM(paid_amount), 0) FROM sales WHERE client_id = c.id AND deleted_at IS NULL {warehouseFilter}) as TotalPaid
            FROM clients c
            WHERE c.deleted_at IS NULL
            ORDER BY TotalAmount DESC";

        return await _uow.Connection.QueryAsync<ClientReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsReportAsync(int limit)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var parameters = new DynamicParameters();
        parameters.Add("limit", limit);

        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND s.warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        string sql = $@"
            SELECT p.name, SUM(sd.quantity) as Quantity, SUM(sd.total) as Total
            FROM sale_details sd
            JOIN products p ON sd.product_id = p.id
            JOIN sales s ON sd.sale_id = s.id
            WHERE s.deleted_at IS NULL {warehouseFilter}
            GROUP BY p.name
            ORDER BY Total DESC
            LIMIT @limit";
        return await _uow.Connection.QueryAsync<TopProductDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<ProductMovementReportDto>> GetProductMovementsReportAsync(long productId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var parameters = new DynamicParameters();
        parameters.Add("productId", productId);

        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        string sql = $@"
            SELECT date::timestamp as ""Date"", ref as ""Ref"", 'Sale' as ""Type"", quantity as ""Quantity"", warehouse_name as ""WarehouseName""
            FROM (SELECT s.date, s.ref, sd.quantity, w.name as warehouse_name FROM sale_details sd JOIN sales s ON sd.sale_id = s.id JOIN warehouses w ON s.warehouse_id = w.id WHERE sd.product_id = @productId AND s.deleted_at IS NULL AND w.deleted_at IS NULL {warehouseFilter.Replace("warehouse_id", "s.warehouse_id")}) as movements
            UNION ALL
            SELECT date::timestamp as ""Date"", ref as ""Ref"", 'Purchase' as ""Type"", quantity as ""Quantity"", warehouse_name as ""WarehouseName""
            FROM (SELECT p.date, p.ref, pd.quantity, w.name as warehouse_name FROM purchase_details pd JOIN purchases p ON pd.purchase_id = p.id JOIN warehouses w ON p.warehouse_id = w.id WHERE pd.product_id = @productId AND p.deleted_at IS NULL AND w.deleted_at IS NULL {warehouseFilter.Replace("warehouse_id", "p.warehouse_id")}) as movements
            ORDER BY ""Date"" DESC";
        return await _uow.Connection.QueryAsync<ProductMovementReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<ProviderReportDto>> GetProviderReportAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var parameters = new DynamicParameters();
        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        string sql = $@"
            SELECT 
                p.id as ProviderId,
                p.name as ProviderName,
                p.phone as Phone,
                p.email as Email,
                (SELECT COUNT(*) FROM purchases WHERE provider_id = p.id AND deleted_at IS NULL {warehouseFilter}) as TotalPurchases,
                (SELECT COALESCE(SUM(grand_total), 0) FROM purchases WHERE provider_id = p.id AND deleted_at IS NULL {warehouseFilter}) as TotalAmount,
                (SELECT COALESCE(SUM(paid_amount), 0) FROM purchases WHERE provider_id = p.id AND deleted_at IS NULL {warehouseFilter}) as TotalPaid
            FROM providers p
            WHERE p.deleted_at IS NULL
            ORDER BY TotalAmount DESC";

        return await _uow.Connection.QueryAsync<ProviderReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<StockAlertReportDto>> GetStockAlertsReportAsync()
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var parameters = new DynamicParameters();
        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND pw.warehouse_id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }

        string sql = $@"
            SELECT p.code, p.name, pw.qty as quantity, p.stock_alert as StockAlert
            FROM products p
            JOIN product_warehouse pw ON p.id = pw.product_id
            JOIN warehouses w ON pw.warehouse_id = w.id
            WHERE pw.qty <= p.stock_alert AND p.deleted_at IS NULL AND w.deleted_at IS NULL {warehouseFilter}";
        
        return await _uow.Connection.QueryAsync<StockAlertReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<ActivityReportDto>> GetActivityReportAsync(long? userId, long? warehouseId, long? productId)
    {
        var activeWarehouseId = _currentUserService.ActiveWarehouseId;
        var hasAllAccess = _currentUserService.HasAllWarehousesAccess;

        var parameters = new DynamicParameters();
        var sqlBuilder = new StringBuilder();

        string warehouseFilter = "";
        if (!hasAllAccess)
        {
            warehouseFilter = " AND w.id = @activeWarehouseId";
            parameters.Add("activeWarehouseId", activeWarehouseId);
        }
        else if (warehouseId.HasValue)
        {
            warehouseFilter = " AND w.id = @warehouseId";
            parameters.Add("warehouseId", warehouseId.Value);
        }

        string userFilter = "";
        if (userId.HasValue) 
        { 
            userFilter = " AND u.id = @userId"; 
            parameters.Add("userId", userId.Value); 
        }

        sqlBuilder.Append($@"
            SELECT s.date::timestamp as ""Date"", s.ref as ""Ref"", 'Sale' as ""Type"", s.grand_total as ""Total"", w.name as ""WarehouseName"", u.username as ""UserName"" 
            FROM sales s 
            JOIN warehouses w ON s.warehouse_id = w.id 
            JOIN users u ON s.user_id = u.id WHERE s.deleted_at IS NULL AND w.deleted_at IS NULL {warehouseFilter} {userFilter}
            UNION ALL
            SELECT p.date::timestamp as ""Date"", p.ref as ""Ref"", 'Purchase' as ""Type"", p.grand_total as ""Total"", w.name as ""WarehouseName"", u.username as ""UserName"" 
            FROM purchases p 
            JOIN warehouses w ON p.warehouse_id = w.id 
            JOIN users u ON p.user_id = u.id WHERE p.deleted_at IS NULL AND w.deleted_at IS NULL {warehouseFilter} {userFilter}
            UNION ALL
            SELECT t.date::timestamp as ""Date"", t.ref as ""Ref"", 'Transfer' as ""Type"", t.grand_total as ""Total"", w.name as ""WarehouseName"", u.username as ""UserName"" 
            FROM transfers t 
            JOIN warehouses w ON t.to_warehouse_id = w.id 
            JOIN users u ON t.user_id = u.id WHERE t.deleted_at IS NULL AND w.deleted_at IS NULL {warehouseFilter} {userFilter}
            ");

        sqlBuilder.Append(@" ORDER BY ""Date"" DESC");

        return await _uow.Connection.QueryAsync<ActivityReportDto>(sqlBuilder.ToString(), parameters, _uow.Transaction);
    }
}
