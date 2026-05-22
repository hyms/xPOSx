using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using XPos.Domain.Dtos;
using FluentAssertions;

namespace XPos.Tests;

public class TransferServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITransferRepository> _transferRepoMock;
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly Mock<IUnitRepository> _unitRepoMock;
    private readonly Mock<ICashShiftRepository> _cashShiftRepoMock;
    private readonly TransferService _transferService;

    public TransferServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _transferRepoMock = new Mock<ITransferRepository>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _unitRepoMock = new Mock<IUnitRepository>();
        _cashShiftRepoMock = new Mock<ICashShiftRepository>();

        _transferService = new TransferService(
            _uowMock.Object,
            _transferRepoMock.Object,
            _inventoryRepoMock.Object,
            _unitRepoMock.Object,
            new UnitConversionService(),
            _cashShiftRepoMock.Object
        );
    }

    [Fact]
    public async Task CreateTransferAsync_ShouldUpdateBothWarehouses()
    {
        // Arrange
        var userId = 1L;
        var dto = new CreateTransferDto
        {
            FromWarehouseId = 1,
            ToWarehouseId = 2,
            Details = new List<CreateTransferDetailDto>
            {
                new CreateTransferDetailDto { ProductId = 1, Quantity = 5, Cost = 10 }
            }
        };

        _transferRepoMock.Setup(x => x.CreateAsync(It.IsAny<Transfer>())).ReturnsAsync(600L);

        // Act
        var result = await _transferService.CreateTransferAsync(dto, userId);

        // Assert
        result.Should().Be(600L);
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, -5), Times.Once); // From
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 2, 5), Times.Once);  // To
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepo()
    {
        await _transferService.GetAllAsync("filter");
        _transferRepoMock.Verify(x => x.GetAllAsync("filter"), Times.Once);
    }

    [Fact]
    public async Task DeleteTransferAsync_ShouldCallRepo()
    {
        var transfer = new Transfer { Id = 1, FromWarehouseId = 10, ToWarehouseId = 20, Date = DateTime.Now, Details = new List<TransferDetail>() };
        _transferRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(transfer);
        _cashShiftRepoMock.Setup(x => x.IsWarehouseClosedForDateAsync(It.IsAny<long>(), It.IsAny<DateTime>())).ReturnsAsync(false);
        _transferRepoMock.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(true);

        await _transferService.DeleteTransferAsync(1, 1);
        _transferRepoMock.Verify(x => x.DeleteAsync(1, 1), Times.Once);
    }
}
