using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using FluentAssertions;

namespace XPos.Tests;

public class PaymentGatewaySettingsServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IPaymentGatewaySettingsRepository> _repoMock;
    private readonly PaymentGatewaySettingsService _service;

    public PaymentGatewaySettingsServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repoMock = new Mock<IPaymentGatewaySettingsRepository>();
        _service = new PaymentGatewaySettingsService(_uowMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCommit()
    {
        _repoMock.Setup(x => x.CreateAsync(It.IsAny<PaymentGatewaySettings>())).ReturnsAsync(1L);
        await _service.CreateAsync(new PaymentGatewaySettings());
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepo()
    {
        await _service.GetAllAsync();
        _repoMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepo()
    {
        await _service.GetByIdAsync(1);
        _repoMock.Verify(x => x.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCommit()
    {
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<PaymentGatewaySettings>())).ReturnsAsync(true);
        await _service.UpdateAsync(new PaymentGatewaySettings());
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCommit()
    {
        _repoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        await _service.DeleteAsync(1);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }
}
