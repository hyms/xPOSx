using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Domain.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;

    // We don't inject IUnitOfWork here currently because reports are read-only 
    // and don't require explicit transaction management across multiple repositories.
    // However, the repository uses it for the connection.
    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IEnumerable<SalesReportDto>> GetSalesReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId)
    {
        return await _reportRepository.GetSalesReportAsync(startDate, endDate, warehouseId);
    }

    public async Task<IEnumerable<PurchaseReportDto>> GetPurchaseReportAsync(DateTime? startDate, DateTime? endDate, long? warehouseId)
    {
        return await _reportRepository.GetPurchaseReportAsync(startDate, endDate, warehouseId);
    }

    public async Task<IEnumerable<StockReportDto>> GetStockReportAsync(long? warehouseId)
    {
        return await _reportRepository.GetStockReportAsync(warehouseId);
    }

    public async Task<ProfitLossReportDto> GetProfitLossReportAsync(DateTime? startDate, DateTime? endDate)
    {
        return await _reportRepository.GetProfitLossReportAsync(startDate, endDate);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        return await _reportRepository.GetDashboardSummaryAsync();
    }

    public async Task<IEnumerable<ClientReportDto>> GetClientReportAsync()
    {
        return await _reportRepository.GetClientReportAsync();
    }

    public async Task<IEnumerable<ProviderReportDto>> GetProviderReportAsync()
    {
        return await _reportRepository.GetProviderReportAsync();
    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsReportAsync(int limit)
    {
        return await _reportRepository.GetTopProductsReportAsync(limit);
    }

    public async Task<IEnumerable<ProductMovementReportDto>> GetProductMovementsReportAsync(long productId)
    {
        return await _reportRepository.GetProductMovementsReportAsync(productId);
    }

    public async Task<IEnumerable<StockAlertReportDto>> GetStockAlertsReportAsync()
    {
        return await _reportRepository.GetStockAlertsReportAsync();
    }

    public async Task<IEnumerable<ActivityReportDto>> GetActivityReportAsync(long? userId, long? warehouseId, long? productId)
    {
        return await _reportRepository.GetActivityReportAsync(userId, warehouseId, productId);
    }
}
