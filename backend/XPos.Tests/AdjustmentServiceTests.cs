using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using XPos.Domain.Dtos;
using FluentAssertions;

namespace XPos.Tests;

public class AdjustmentServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IAdjustmentRepository> _adjustmentRepoMock;
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly AdjustmentService _adjustmentService;

    public AdjustmentServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _adjustmentRepoMock = new Mock<IAdjustmentRepository>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        
        _adjustmentService = new AdjustmentService(
            _uowMock.Object,
            _adjustmentRepoMock.Object,
            _inventoryRepoMock.Object
        );
    }

    [Fact]
    public async Task CreateAdjustmentAsync_ShouldUpdateStockAndCommit()
    {
        // Arrange
        var userId = 1L;
        var dto = new CreateAdjustmentDto
        {
            WarehouseId = 1,
            Date = DateTime.Now,
            Details = new List<CreateAdjustmentDetailDto>
            {
                new CreateAdjustmentDetailDto { ProductId = 1, Quantity = 10, Type = "add" },
                new CreateAdjustmentDetailDto { ProductId = 2, Quantity = 5, Type = "sub" }
            }
        };

        _adjustmentRepoMock.Setup(x => x.CreateAsync(It.IsAny<Adjustment>())).ReturnsAsync(300L);

        // Act
        var result = await _adjustmentService.CreateAdjustmentAsync(dto, userId);

        // Assert
        result.Should().Be(300L);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, 10), Times.Once);
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(2, 1, -5), Times.Once);
    }

    [Fact]
    public async Task UpdateAdjustmentAsync_ShouldReverseOldStockAndApplyNewStock()
    {
        // Arrange
        var userId = 1L;
        var existing = new Adjustment
        {
            Id = 300,
            WarehouseId = 1,
            Details = new List<AdjustmentDetail>
            {
                new AdjustmentDetail { ProductId = 1, Quantity = 10, Type = "add" }
            }
        };

        var dto = new UpdateAdjustmentDto
        {
            Id = 300,
            WarehouseId = 1,
            Date = DateTime.Now,
            Details = new List<CreateAdjustmentDetailDto>
            {
                new CreateAdjustmentDetailDto { ProductId = 1, Quantity = 15, Type = "add" }
            }
        };

        _adjustmentRepoMock.Setup(x => x.GetByIdAsync(300)).ReturnsAsync(existing);
        _adjustmentRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Adjustment>())).ReturnsAsync(true);

        // Act
        var result = await _adjustmentService.UpdateAdjustmentAsync(dto, userId);

        // Assert
        result.Should().BeTrue();
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, -10), Times.Once); // Reverse
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, 15), Times.Once);  // Apply new
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepo()
    {
        await _adjustmentService.GetAllAsync("filter");
        _adjustmentRepoMock.Verify(x => x.GetAllAsync("filter"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepo()
    {
        await _adjustmentService.GetByIdAsync(1);
        _adjustmentRepoMock.Verify(x => x.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAdjustmentAsync_ShouldCallRepo()
    {
        await _adjustmentService.DeleteAdjustmentAsync(1, 1);
        _adjustmentRepoMock.Verify(x => x.DeleteAsync(1, 1), Times.Once);
    }
}
