using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using XPos.Domain.Dtos;
using FluentAssertions;

namespace XPos.Tests;

public class ReturnServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ISaleReturnRepository> _saleReturnRepoMock;
    private readonly Mock<IPurchaseReturnRepository> _purchaseReturnRepoMock;
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly Mock<IUnitRepository> _unitRepoMock;
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly ReturnService _returnService;

    public ReturnServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _saleReturnRepoMock = new Mock<ISaleReturnRepository>();
        _purchaseReturnRepoMock = new Mock<IPurchaseReturnRepository>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _unitRepoMock = new Mock<IUnitRepository>();
        _voucherRepoMock = new Mock<IVoucherRepository>();

        _returnService = new ReturnService(
            _uowMock.Object,
            _saleReturnRepoMock.Object,
            _purchaseReturnRepoMock.Object,
            _inventoryRepoMock.Object,
            _unitRepoMock.Object,
            new UnitConversionService(),
            _voucherRepoMock.Object
        );
    }

    [Fact]
    public async Task CreateSaleReturnAsync_ShouldIncreaseStock()
    {
        // Arrange
        var userId = 1L;
        var dto = new CreateSaleReturnDto
        {
            WarehouseId = 1,
            Date = DateTime.Now,
            Details = new List<CreateSaleReturnDetailDto>
            {
                new CreateSaleReturnDetailDto { ProductId = 1, Quantity = 2, Price = 50 }
            }
        };

        _saleReturnRepoMock.Setup(x => x.CreateAsync(It.IsAny<SaleReturn>())).ReturnsAsync(400L);

        // Act
        var result = await _returnService.CreateSaleReturnAsync(dto, userId);

        // Assert
        result.Should().Be(400L);
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, 2), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task CreatePurchaseReturnAsync_ShouldDecreaseStock()
    {
        // Arrange
        var userId = 1L;
        var dto = new CreatePurchaseReturnDto
        {
            WarehouseId = 1,
            Date = DateTime.Now,
            Details = new List<CreatePurchaseReturnDetailDto>
            {
                new CreatePurchaseReturnDetailDto { ProductId = 1, Quantity = 3, Cost = 30 }
            }
        };

        _purchaseReturnRepoMock.Setup(x => x.CreateAsync(It.IsAny<PurchaseReturn>())).ReturnsAsync(500L);

        // Act
        var result = await _returnService.CreatePurchaseReturnAsync(dto, userId);

        // Assert
        result.Should().Be(500L);
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, -3), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task GetAllSaleReturnsAsync_ShouldCallRepo()
    {
        await _returnService.GetAllSaleReturnsAsync();
        _saleReturnRepoMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteSaleReturnAsync_ShouldCallRepo()
    {
        await _returnService.DeleteSaleReturnAsync(1, 1);
        _saleReturnRepoMock.Verify(x => x.DeleteAsync(1, 1), Times.Once);
    }
}
