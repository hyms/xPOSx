using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace XPos.Tests;

public class SaleServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ISaleRepository> _saleRepoMock;
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IUnitRepository> _unitRepoMock;
    private readonly Mock<ICashShiftRepository> _cashShiftRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
    private readonly SaleService _saleService;

    public SaleServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _saleRepoMock = new Mock<ISaleRepository>();
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _unitRepoMock = new Mock<IUnitRepository>();
        _cashShiftRepoMock = new Mock<ICashShiftRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _warehouseRepoMock = new Mock<IWarehouseRepository>();
        
        // We can use the real UnitConversionService if it's simple enough or mock it.
        // Let's use the real one to test integration within Domain Services.
        var conversionService = new UnitConversionService();
        var siatLoggerMock = new Mock<ILogger<SiatSoapService>>();
        var siatService = new SiatSoapService(new HttpClient(), siatLoggerMock.Object);
        
        _saleService = new SaleService(
            _uowMock.Object,
            _saleRepoMock.Object,
            _voucherRepoMock.Object,
            _inventoryRepoMock.Object,
            _paymentRepoMock.Object,
            _unitRepoMock.Object,
            conversionService,
            _cashShiftRepoMock.Object,
            _clientRepoMock.Object,
            _warehouseRepoMock.Object,
            siatService
        );
    }

    [Fact]
    public async Task CreateSaleAsync_ShouldCommit_WhenEverythingIsSuccessful()
    {
        // Arrange
        var userId = 1L;
        var sale = new Sale
        {
            WarehouseId = 1,
            Date = DateTime.Now,
            Details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 1, Quantity = 10, SaleUnitId = 1 }
            },
            PaidAmount = 100
        };

        _saleRepoMock.Setup(x => x.CreateAsync(It.IsAny<Sale>())).ReturnsAsync(100L);
        _unitRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Unit { Id = 1, OperatorValue = 1 });

        // Act
        var result = await _saleService.CreateSaleAsync(sale, userId);

        // Assert
        result.Should().Be(100L);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(1, 1, -10), Times.Once);
        _paymentRepoMock.Verify(x => x.CreateSalePaymentAsync(It.IsAny<PaymentSaleDto>()), Times.Once);
    }

    [Fact]
    public async Task CreateSaleAsync_ShouldRollback_WhenExceptionOccurs()
    {
        // Arrange
        var sale = new Sale { Details = new List<SaleDetail>() };
        _saleRepoMock.Setup(x => x.CreateAsync(It.IsAny<Sale>())).ThrowsAsync(new Exception("DB Error"));

        // Act
        Func<Task> act = async () => await _saleService.CreateSaleAsync(sale, 1L);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(x => x.Rollback(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ShouldCallRepo()
    {
        var paging = new PagingParams();
        await _saleService.GetAllAsync(paging);
        _saleRepoMock.Verify(x => x.GetAllAsync(paging), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldCallRepo()
    {
        await _saleService.GetByIdAsync(1);
        _saleRepoMock.Verify(x => x.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteSaleAsync_ShouldRestoreStock_WhenShiftIsOpen()
    {
        // Arrange
        var saleId = 1L;
        var userId = 2L;
        var sale = new Sale
        {
            Id = saleId,
            WarehouseId = 10,
            CashShiftId = 5L,
            Details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 100, Quantity = 3, Price = 50.00m }
            }
        };

        _saleRepoMock.Setup(x => x.GetByIdAsync(saleId)).ReturnsAsync(sale);
        _cashShiftRepoMock.Setup(x => x.GetByIdAsync(5L)).ReturnsAsync(new CashShift { Id = 5L, Status = "OPEN" });
        _saleRepoMock.Setup(x => x.DeleteAsync(saleId, userId)).ReturnsAsync(true);

        // Act
        var result = await _saleService.DeleteSaleAsync(saleId, userId);

        // Assert
        result.Should().BeTrue();
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(100, 10, 3), Times.Once); // Should restore stock (+3)
        _saleRepoMock.Verify(x => x.DeleteAsync(saleId, userId), Times.Once);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task DeleteSaleAsync_ShouldThrow_WhenShiftIsClosed()
    {
        // Arrange
        var saleId = 1L;
        var userId = 2L;
        var sale = new Sale
        {
            Id = saleId,
            WarehouseId = 10,
            CashShiftId = 5L,
            Details = new List<SaleDetail>()
        };

        _saleRepoMock.Setup(x => x.GetByIdAsync(saleId)).ReturnsAsync(sale);
        _cashShiftRepoMock.Setup(x => x.GetByIdAsync(5L)).ReturnsAsync(new CashShift { Id = 5L, Status = "CLOSED" });

        // Act
        Func<Task> act = async () => await _saleService.DeleteSaleAsync(saleId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No se puede eliminar la venta porque pertenece a un turno de caja cerrado.");
        
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<decimal>()), Times.Never);
        _saleRepoMock.Verify(x => x.DeleteAsync(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Rollback(), Times.Once);
    }

    [Fact]
    public async Task VerifyOnlineSaleAsync_ShouldProcessSuccessfully_WhenSaleIsPending()
    {
        // Arrange
        var saleId = 1L;
        var userId = 2L;
        var cashShiftId = 5L;
        var sale = new Sale
        {
            Id = saleId,
            WarehouseId = 10,
            Status = "PENDING_VERIFICATION",
            GrandTotal = 150.00m,
            Details = new List<SaleDetail>
            {
                new SaleDetail { ProductId = 100, Quantity = 3, Price = 50.00m, SaleUnitId = 1 }
            }
        };

        _saleRepoMock.Setup(x => x.GetByIdAsync(saleId)).ReturnsAsync(sale);
        _unitRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Unit { Id = 1, OperatorValue = 1 });
        _saleRepoMock.Setup(x => x.UpdateVerifyStatusAsync(saleId, "PAID", "paid", userId, cashShiftId)).ReturnsAsync(true);

        // Act
        var result = await _saleService.VerifyOnlineSaleAsync(saleId, userId, cashShiftId);

        // Assert
        result.Should().BeTrue();
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(100, 10, -3), Times.Once); // Should discount stock (-3)
        _paymentRepoMock.Verify(x => x.CreateSalePaymentAsync(It.Is<PaymentSaleDto>(p => p.Amount == 150.00m && p.SaleId == saleId)), Times.Once);
        _saleRepoMock.Verify(x => x.UpdateVerifyStatusAsync(saleId, "PAID", "paid", userId, cashShiftId), Times.Once);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task VerifyOnlineSaleAsync_ShouldThrow_WhenSaleIsNotPending()
    {
        // Arrange
        var saleId = 1L;
        var userId = 2L;
        var cashShiftId = 5L;
        var sale = new Sale
        {
            Id = saleId,
            WarehouseId = 10,
            Status = "PAID",
            Details = new List<SaleDetail>()
        };

        _saleRepoMock.Setup(x => x.GetByIdAsync(saleId)).ReturnsAsync(sale);

        // Act
        Func<Task> act = async () => await _saleService.VerifyOnlineSaleAsync(saleId, userId, cashShiftId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("La venta no se encuentra en estado pendiente de verificación.");
        
        _inventoryRepoMock.Verify(x => x.UpdateStockAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<decimal>()), Times.Never);
        _saleRepoMock.Verify(x => x.UpdateVerifyStatusAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }
}
