using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using FluentAssertions;

namespace XPos.Tests;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _repoMock;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _repoMock = new Mock<IReportRepository>();
        _service = new ReportService(_repoMock.Object);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_CallsRepo()
    {
        _repoMock.Setup(x => x.GetDashboardSummaryAsync()).ReturnsAsync(new DashboardSummaryDto());
        var result = await _service.GetDashboardSummaryAsync();
        result.Should().NotBeNull();
        _repoMock.Verify(x => x.GetDashboardSummaryAsync(), Times.Once);
    }

    [Fact]
    public async Task AllOtherMethods_ShouldCallRepo()
    {
        await _service.GetSalesReportAsync(null, null, null);
        _repoMock.Verify(x => x.GetSalesReportAsync(null, null, null), Times.Once);

        await _service.GetPurchaseReportAsync(null, null, null);
        _repoMock.Verify(x => x.GetPurchaseReportAsync(null, null, null), Times.Once);

        await _service.GetStockReportAsync(null);
        _repoMock.Verify(x => x.GetStockReportAsync(null), Times.Once);

        await _service.GetProfitLossReportAsync(null, null);
        _repoMock.Verify(x => x.GetProfitLossReportAsync(null, null), Times.Once);

        await _service.GetClientReportAsync();
        _repoMock.Verify(x => x.GetClientReportAsync(), Times.Once);

        await _service.GetProviderReportAsync();
        _repoMock.Verify(x => x.GetProviderReportAsync(), Times.Once);

        await _service.GetTopProductsReportAsync(10);
        _repoMock.Verify(x => x.GetTopProductsReportAsync(10), Times.Once);

        await _service.GetProductMovementsReportAsync(1);
        _repoMock.Verify(x => x.GetProductMovementsReportAsync(1), Times.Once);

        await _service.GetStockAlertsReportAsync();
        _repoMock.Verify(x => x.GetStockAlertsReportAsync(), Times.Once);

        await _service.GetActivityReportAsync(null, null, null);
        _repoMock.Verify(x => x.GetActivityReportAsync(null, null, null), Times.Once);
    }
}
