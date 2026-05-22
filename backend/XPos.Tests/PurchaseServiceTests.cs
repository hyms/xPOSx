using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using XPos.Domain.Dtos;
using FluentAssertions;

namespace XPos.Tests;

public class PurchaseServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IPurchaseRepository> _purchaseRepoMock;
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IUnitRepository> _unitRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<ICashShiftRepository> _cashShiftRepoMock;
    private readonly PurchaseService _purchaseService;

    public PurchaseServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _purchaseRepoMock = new Mock<IPurchaseRepository>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _unitRepoMock = new Mock<IUnitRepository>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _cashShiftRepoMock = new Mock<ICashShiftRepository>();
        
        _purchaseService = new PurchaseService(
            _uowMock.Object,
            _purchaseRepoMock.Object,
            _inventoryRepoMock.Object,
            _productRepoMock.Object,
            _unitRepoMock.Object,
            new UnitConversionService(),
            _paymentRepoMock.Object,
            _voucherRepoMock.Object,
            _cashShiftRepoMock.Object
        );
    }

    [Fact]
    public async Task CreatePurchaseAsync_ShouldIncreaseStockAndCommit()
    {
        // Arrange
        var userId = 1L;
        var dto = new CreatePurchaseDto
        {
            WarehouseId = 1,
            Date = DateTime.Now,
            Details = new List<CreatePurchaseDetailDto>
            {
                new CreatePurchaseDetailDto { ProductId = 1, Quantity = 5, Cost = 20, PurchaseUnitId = 1 }
            }
        };

        _purchaseRepoMock.Setup(x => x.CreateAsync(It.IsAny<Purchase>())).ReturnsAsync(200L);
        _unitRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Unit { Id = 1, Operator = "*", OperatorValue = 2 }); // e.g. Pack of 2

        // Act
        var result = await _purchaseService.CreatePurchaseAsync(dto, userId);

        // Assert
        result.Should().Be(200L);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, 10), Times.Once); // 5 * 2 = 10
        _productRepoMock.Verify(x => x.UpdateCostAsync(1, 20), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepo()
    {
        var paging = new PagingParams();
        await _purchaseService.GetAllAsync(paging);
        _purchaseRepoMock.Verify(x => x.GetAllAsync(paging), Times.Once);
    }

    [Fact]
    public async Task DeletePurchaseAsync_ShouldCallRepo()
    {
        var purchase = new Purchase { Id = 1, WarehouseId = 10, Date = DateTime.Now, Details = new List<PurchaseDetail>() };
        _purchaseRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(purchase);
        _cashShiftRepoMock.Setup(x => x.IsWarehouseClosedForDateAsync(10, It.IsAny<DateTime>())).ReturnsAsync(false);
        _purchaseRepoMock.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(true);

        await _purchaseService.DeletePurchaseAsync(1, 1);
        _purchaseRepoMock.Verify(x => x.DeleteAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task UpdatePurchaseAsync_ShouldCallRepo()
    {
        var dto = new UpdatePurchaseDto { Id = 1, Details = new List<CreatePurchaseDetailDto>() };
        await _purchaseService.UpdatePurchaseAsync(dto, 1);
        _purchaseRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Purchase>()), Times.Once);
    }
}
