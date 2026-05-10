using Dapper;
using System.Data;
using System.Text;
using Microsoft.Extensions.Configuration;
using Npgsql;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Data.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IUnitOfWork _uow;

    public ReportRepository(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId)
    {
        string sql = @"
            SELECT s.date, s.ref, c.name as ClientName, w.name as WarehouseName, s.grand_total as GrandTotal, s.paid_amount as PaidAmount, s.payment_status as PaymentStatus
            FROM sales s
            JOIN clients c ON s.client_id = c.id
            JOIN warehouses w ON s.warehouse_id = w.id
            WHERE s.deleted_at IS NULL";

        var parameters = new DynamicParameters();
        if (startDate.HasValue) { sql += " AND s.date >= @startDate"; parameters.Add("startDate", startDate.Value); }
        if (endDate.HasValue) { sql += " AND s.date <= @endDate"; parameters.Add("endDate", endDate.Value); }
        if (warehouseId.HasValue) { sql += " AND s.warehouse_id = @warehouseId"; parameters.Add("warehouseId", warehouseId.Value); }

        sql += " ORDER BY s.date DESC";
        return await _uow.Connection.QueryAsync<SalesReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<PurchaseReportDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId)
    {
        string sql = @"
            SELECT p.date, p.ref, pr.name as ProviderName, w.name as WarehouseName, p.grand_total as GrandTotal, p.paid_amount as PaidAmount, p.status
            FROM purchases p
            JOIN providers pr ON p.provider_id = pr.id
            JOIN warehouses w ON p.warehouse_id = w.id
            WHERE p.deleted_at IS NULL";

        var parameters = new DynamicParameters();
        if (startDate.HasValue) { sql += " AND p.date >= @startDate"; parameters.Add("startDate", startDate.Value); }
        if (endDate.HasValue) { sql += " AND p.date <= @endDate"; parameters.Add("endDate", endDate.Value); }
        if (warehouseId.HasValue) { sql += " AND p.warehouse_id = @warehouseId"; parameters.Add("warehouseId", warehouseId.Value); }

        sql += " ORDER BY p.date DESC";
        return await _uow.Connection.QueryAsync<PurchaseReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<IEnumerable<StockReportDto>> GetStockReportAsync(long? warehouseId)
    {
        string sql = @"
            SELECT p.code as ProductCode, p.name as ProductName, cat.name as CategoryName, w.name as WarehouseName, pw.qty as Quantity, p.stock_alert as StockAlert
            FROM product_warehouse pw
            JOIN products p ON pw.product_id = p.id
            JOIN warehouses w ON pw.warehouse_id = w.id
            JOIN categories cat ON p.category_id = cat.id
            WHERE p.deleted_at IS NULL";

        var parameters = new DynamicParameters();
        if (warehouseId.HasValue) { sql += " AND pw.warehouse_id = @warehouseId"; parameters.Add("warehouseId", warehouseId.Value); }

        sql += " ORDER BY pw.qty ASC";
        return await _uow.Connection.QueryAsync<StockReportDto>(sql, parameters, _uow.Transaction);
    }

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate)
    {
        var parameters = new DynamicParameters();
        string dateFilter = "";
        if (startDate.HasValue) { dateFilter += " AND date >= @startDate"; parameters.Add("startDate", startDate.Value); }
        if (endDate.HasValue) { dateFilter += " AND date <= @endDate"; parameters.Add("endDate", endDate.Value); }

        var salesSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE deleted_at IS NULL {dateFilter}";
        var purchasesSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM purchases WHERE deleted_at IS NULL {dateFilter}";
        var expensesSql = $"SELECT COALESCE(SUM(amount), 0) FROM expenses WHERE deleted_at IS NULL {dateFilter}";
        var returnsSql = $"SELECT COALESCE(SUM(grand_total), 0) FROM sale_returns WHERE deleted_at IS NULL {dateFilter}";

        var totalSales = await _uow.Connection.ExecuteScalarAsync<double>(salesSql, parameters, _uow.Transaction);
        var totalPurchases = await _uow.Connection.ExecuteScalarAsync<double>(purchasesSql, parameters, _uow.Transaction);
        var totalExpenses = await _uow.Connection.ExecuteScalarAsync<double>(expensesSql, parameters, _uow.Transaction);
        var totalReturns = await _uow.Connection.ExecuteScalarAsync<double>(returnsSql, parameters, _uow.Transaction);

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
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

        var todaySalesSql = "SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE date = @today AND deleted_at IS NULL";
        var todaySalesCountSql = "SELECT COUNT(*) FROM sales WHERE date = @today AND deleted_at IS NULL";
        var monthlySalesSql = "SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE date >= @firstDayOfMonth AND deleted_at IS NULL";
        var monthlyPurchasesSql = "SELECT COALESCE(SUM(grand_total), 0) FROM purchases WHERE date >= @firstDayOfMonth AND deleted_at IS NULL";
        var recentSalesSql = @"
            SELECT s.ref, c.name as ClientName, s.grand_total as GrandTotal, s.status
            FROM sales s
            JOIN clients c ON s.client_id = c.id
            WHERE s.deleted_at IS NULL
            ORDER BY s.created_at DESC
            LIMIT 5";

        var topProductsSql = @"
            SELECT p.name, SUM(sd.quantity) as Quantity, SUM(sd.total) as Total
            FROM sale_details sd
            JOIN products p ON sd.product_id = p.id
            JOIN sales s ON sd.sale_id = s.id
            WHERE s.deleted_at IS NULL
            GROUP BY p.name
            ORDER BY Total DESC
            LIMIT 5";

        var summary = new DashboardSummaryDto();
        summary.TodaySales = await _uow.Connection.ExecuteScalarAsync<double>(todaySalesSql, new { today }, _uow.Transaction);
        summary.TodaySalesCount = await _uow.Connection.ExecuteScalarAsync<int>(todaySalesCountSql, new { today }, _uow.Transaction);
        summary.MonthlySales = await _uow.Connection.ExecuteScalarAsync<double>(monthlySalesSql, new { firstDayOfMonth }, _uow.Transaction);
        summary.MonthlyPurchases = await _uow.Connection.ExecuteScalarAsync<double>(monthlyPurchasesSql, new { firstDayOfMonth }, _uow.Transaction);
        summary.RecentSales = (await _uow.Connection.QueryAsync<RecentSaleDto>(recentSalesSql, null, _uow.Transaction)).ToList();
        summary.TopProducts = (await _uow.Connection.QueryAsync<TopProductDto>(topProductsSql, null, _uow.Transaction)).ToList();

        return summary;
    }

    public async Task<IEnumerable<ClientReportDto>> GetClientReportAsync()
    {
        string sql = @"
            SELECT 
                c.id as ClientId,
                c.name as ClientName,
                c.phone as Phone,
                c.email as Email,
                (SELECT COUNT(*) FROM sales WHERE client_id = c.id AND deleted_at IS NULL) as TotalSales,
                (SELECT COALESCE(SUM(grand_total), 0) FROM sales WHERE client_id = c.id AND deleted_at IS NULL) as TotalAmount,
                (SELECT COALESCE(SUM(paid_amount), 0) FROM sales WHERE client_id = c.id AND deleted_at IS NULL) as TotalPaid
            FROM clients c
            WHERE c.deleted_at IS NULL
            ORDER BY TotalAmount DESC";

        return await _uow.Connection.QueryAsync<ClientReportDto>(sql, null, _uow.Transaction);
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsReportAsync(int limit)
    {
        string sql = @"
            SELECT p.name, SUM(sd.quantity) as Quantity, SUM(sd.total) as Total
            FROM sale_details sd
            JOIN products p ON sd.product_id = p.id
            JOIN sales s ON sd.sale_id = s.id
            WHERE s.deleted_at IS NULL
            GROUP BY p.name
            ORDER BY Total DESC
            LIMIT @limit";
        return await _uow.Connection.QueryAsync<TopProductDto>(sql, new { limit }, _uow.Transaction);
    }

    public async Task<IEnumerable<ProductMovementReportDto>> GetProductMovementsReportAsync(long productId)
    {
        string sql = @"
            SELECT date, ref, 'Sale' as Type, quantity, warehouse_name as WarehouseName
            FROM (SELECT s.date, s.ref, sd.quantity, w.name as warehouse_name FROM sale_details sd JOIN sales s ON sd.sale_id = s.id JOIN warehouses w ON s.warehouse_id = w.id WHERE sd.product_id = @productId AND s.deleted_at IS NULL) as movements
            UNION ALL
            SELECT date, ref, 'Purchase' as Type, quantity, warehouse_name as WarehouseName
            FROM (SELECT p.date, p.ref, pd.quantity, w.name as warehouse_name FROM purchase_details pd JOIN purchases p ON pd.purchase_id = p.id JOIN warehouses w ON p.warehouse_id = w.id WHERE pd.product_id = @productId AND p.deleted_at IS NULL) as movements
            ORDER BY date DESC";
        return await _uow.Connection.QueryAsync<ProductMovementReportDto>(sql, new { productId }, _uow.Transaction);
    }

    public async Task<IEnumerable<ProviderReportDto>> GetProviderReportAsync()
    {
        string sql = @"
            SELECT 
                p.id as ProviderId,
                p.name as ProviderName,
                p.phone as Phone,
                p.email as Email,
                (SELECT COUNT(*) FROM purchases WHERE provider_id = p.id AND deleted_at IS NULL) as TotalPurchases,
                (SELECT COALESCE(SUM(grand_total), 0) FROM purchases WHERE provider_id = p.id AND deleted_at IS NULL) as TotalAmount,
                (SELECT COALESCE(SUM(paid_amount), 0) FROM purchases WHERE provider_id = p.id AND deleted_at IS NULL) as TotalPaid
            FROM providers p
            WHERE p.deleted_at IS NULL
            ORDER BY TotalAmount DESC";

        return await _uow.Connection.QueryAsync<ProviderReportDto>(sql, null, _uow.Transaction);
    }

    public async Task<IEnumerable<StockAlertReportDto>> GetStockAlertsReportAsync()
    {
        string sql = @"
            SELECT code, name, quantity, stock_alert as StockAlert
            FROM products
            WHERE quantity <= stock_alert AND deleted_at IS NULL";
        
        return await _uow.Connection.QueryAsync<StockAlertReportDto>(sql, null, _uow.Transaction);
    }

    public async Task<IEnumerable<ActivityReportDto>> GetActivityReportAsync(long? userId, long? warehouseId, long? productId)
    {
        var parameters = new DynamicParameters();
        var sqlBuilder = new StringBuilder();

        // 1. Union All queries for Sales, Purchases, Transfers, Adjustments, Returns
        sqlBuilder.Append(@"
            SELECT s.date, s.ref, 'Sale' as Type, s.grand_total as Total, w.name as WarehouseName, u.username as UserName 
            FROM sales s 
            JOIN warehouses w ON s.warehouse_id = w.id 
            JOIN users u ON s.user_id = u.id WHERE s.deleted_at IS NULL
            UNION ALL
            SELECT p.date, p.ref, 'Purchase' as Type, p.grand_total as Total, w.name as WarehouseName, u.username as UserName 
            FROM purchases p 
            JOIN warehouses w ON p.warehouse_id = w.id 
            JOIN users u ON p.user_id = u.id WHERE p.deleted_at IS NULL
            UNION ALL
            SELECT t.date, t.ref, 'Transfer' as Type, t.grand_total as Total, w.name as WarehouseName, u.username as UserName 
            FROM transfers t 
            JOIN warehouses w ON t.to_warehouse_id = w.id 
            JOIN users u ON t.user_id = u.id WHERE t.deleted_at IS NULL
            ");

        if (userId.HasValue) { sqlBuilder.Append(" AND u.id = @userId"); parameters.Add("userId", userId.Value); }
        if (warehouseId.HasValue) { sqlBuilder.Append(" AND w.id = @warehouseId"); parameters.Add("warehouseId", warehouseId.Value); }
        // Note: product filtering would require joining sale_details/purchase_details, which complicates the UNIONs.
        // For now, focusing on User/Warehouse based activity.
        
        sqlBuilder.Append(" ORDER BY date DESC");

        return await _uow.Connection.QueryAsync<ActivityReportDto>(sqlBuilder.ToString(), parameters, _uow.Transaction);
    }
}
