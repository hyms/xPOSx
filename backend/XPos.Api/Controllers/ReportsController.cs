using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    public ReportsController(IReportService reportService) { _reportService = reportService; }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] long? warehouseId)
    {
        return Ok(await _reportService.GetSalesReportAsync(startDate, endDate, warehouseId));
    }

    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchaseReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] long? warehouseId)
    {
        return Ok(await _reportService.GetPurchaseReportAsync(startDate, endDate, warehouseId));
    }

    [HttpGet("stock")]
    public async Task<IActionResult> GetStockReport([FromQuery] long? warehouseId)
    {
        return Ok(await _reportService.GetStockReportAsync(warehouseId));
    }

    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLossReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return Ok(await _reportService.GetProfitLossReportAsync(startDate, endDate));
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        return Ok(await _reportService.GetDashboardSummaryAsync());
    }

    [HttpGet("clients")]
    public async Task<IActionResult> GetClientReport()
    {
        return Ok(await _reportService.GetClientReportAsync());
    }

    [HttpGet("providers")]
    public async Task<IActionResult> GetProviderReport()
    {
        return Ok(await _reportService.GetProviderReportAsync());
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 10)
    {
        return Ok(await _reportService.GetTopProductsReportAsync(limit));
    }

    [HttpGet("product-movements/{productId}")]
    public async Task<IActionResult> GetProductMovements(long productId)
    {
        return Ok(await _reportService.GetProductMovementsReportAsync(productId));
    }

    [HttpGet("stock-alerts")]
    public async Task<IActionResult> GetStockAlerts()
    {
        return Ok(await _reportService.GetStockAlertsReportAsync());
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivityReport(
        [FromQuery] long? userId, 
        [FromQuery] long? warehouseId, 
        [FromQuery] long? productId)
    {
        return Ok(await _reportService.GetActivityReportAsync(userId, warehouseId, productId));
    }
}
