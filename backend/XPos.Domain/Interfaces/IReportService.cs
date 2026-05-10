using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Models;

namespace XPos.Domain.Interfaces;

public interface IReportService
{
    Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId);
    Task<IEnumerable<PurchaseReportDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId);
    Task<IEnumerable<StockReportDto>> GetStockReportAsync(long? warehouseId);
    Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<IEnumerable<ClientReportDto>> GetClientReportAsync();
    Task<IEnumerable<ProviderReportDto>> GetProviderReportAsync();
    Task<IEnumerable<TopProductDto>> GetTopProductsReportAsync(int limit);
    Task<IEnumerable<ProductMovementReportDto>> GetProductMovementsReportAsync(long productId);
    Task<IEnumerable<StockAlertReportDto>> GetStockAlertsReportAsync();
    Task<IEnumerable<ActivityReportDto>> GetActivityReportAsync(long? userId, long? warehouseId, long? productId);
}
