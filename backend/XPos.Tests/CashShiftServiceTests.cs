using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using Xunit;

namespace XPos.Tests;

public class CashShiftServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICashShiftRepository> _cashShiftRepoMock;
    private readonly Mock<ICashRegisterRepository> _cashRegisterRepoMock;
    private readonly Mock<ICashTransactionRepository> _cashTransactionRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly CashShiftService _cashShiftService;

    public CashShiftServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _cashShiftRepoMock = new Mock<ICashShiftRepository>();
        _cashRegisterRepoMock = new Mock<ICashRegisterRepository>();
        _cashTransactionRepoMock = new Mock<ICashTransactionRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        _cashShiftService = new CashShiftService(
            _uowMock.Object,
            _cashShiftRepoMock.Object,
            _cashRegisterRepoMock.Object,
            _cashTransactionRepoMock.Object,
            _userRepoMock.Object
        );
    }

    [Fact]
    public async Task OpenShiftAsync_ShouldCreateShift_WhenValid()
    {
        // Arrange
        var registerId = 1L;
        var userId = 2L;
        var startingCash = 100.00m;
        var activeWarehouseId = 10L;

        var register = new CashRegister { Id = registerId, Name = "Caja 1", WarehouseId = activeWarehouseId, IsActive = true };
        _cashRegisterRepoMock.Setup(x => x.GetByIdAsync(registerId)).ReturnsAsync(register);
        _cashShiftRepoMock.Setup(x => x.GetActiveShiftAsync(userId, activeWarehouseId)).ReturnsAsync((CashShift?)null);
        _cashShiftRepoMock.Setup(x => x.GetActiveShiftByRegisterIdAsync(registerId)).ReturnsAsync((CashShift?)null);
        _cashShiftRepoMock.Setup(x => x.CreateAsync(It.IsAny<CashShift>())).ReturnsAsync(5L);

        // Act
        var result = await _cashShiftService.OpenShiftAsync(registerId, userId, startingCash, activeWarehouseId);

        // Assert
        result.Should().Be(5L);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task OpenShiftAsync_ShouldThrow_WhenUserHasActiveShift()
    {
        // Arrange
        var registerId = 1L;
        var userId = 2L;
        var startingCash = 100.00m;
        var activeWarehouseId = 10L;

        var register = new CashRegister { Id = registerId, Name = "Caja 1", WarehouseId = activeWarehouseId, IsActive = true };
        _cashRegisterRepoMock.Setup(x => x.GetByIdAsync(registerId)).ReturnsAsync(register);
        _cashShiftRepoMock.Setup(x => x.GetActiveShiftAsync(userId, activeWarehouseId)).ReturnsAsync(new CashShift());

        // Act
        Func<Task> act = async () => await _cashShiftService.OpenShiftAsync(registerId, userId, startingCash, activeWarehouseId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("El usuario ya tiene un turno de caja abierto en este almacén.");
        _uowMock.Verify(x => x.BeginTransaction(), Times.Never);
    }

    [Fact]
    public async Task OpenShiftAsync_ShouldThrow_WhenRegisterIsOccupied()
    {
        // Arrange
        var registerId = 1L;
        var userId = 2L;
        var startingCash = 100.00m;
        var activeWarehouseId = 10L;

        var register = new CashRegister { Id = registerId, Name = "Caja 1", WarehouseId = activeWarehouseId, IsActive = true };
        _cashRegisterRepoMock.Setup(x => x.GetByIdAsync(registerId)).ReturnsAsync(register);
        _cashShiftRepoMock.Setup(x => x.GetActiveShiftAsync(userId, activeWarehouseId)).ReturnsAsync((CashShift?)null);
        _cashShiftRepoMock.Setup(x => x.GetActiveShiftByRegisterIdAsync(registerId)).ReturnsAsync(new CashShift());

        // Act
        Func<Task> act = async () => await _cashShiftService.OpenShiftAsync(registerId, userId, startingCash, activeWarehouseId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("La caja seleccionada ya tiene un turno activo abierto por otro usuario.");
        _uowMock.Verify(x => x.BeginTransaction(), Times.Never);
    }

    [Fact]
    public async Task ExecuteManualTransactionAsync_ShouldCreateTransaction_AndGenerateCorrectVoucherNumber()
    {
        // Arrange
        var shiftId = 5L;
        var userId = 2L;
        var shift = new CashShift { Id = shiftId, Status = "OPEN" };
        _cashShiftRepoMock.Setup(x => x.GetByIdAsync(shiftId)).ReturnsAsync(shift);
        _cashTransactionRepoMock.Setup(x => x.GetDailyTransactionCountAsync()).ReturnsAsync(4); // 4 existing daily transactions

        // Act
        var result = await _cashShiftService.ExecuteManualTransactionAsync(shiftId, "CASH_DROP", 250.00m, "Remesa", "Admin", userId);

        // Assert
        var expectedDate = DateTime.UtcNow.ToString("yyyyMMdd");
        result.Should().Be($"VOUCHER-CASH-{expectedDate}-0005"); // count (4) + 1 = 5
        _cashTransactionRepoMock.Verify(x => x.CreateAsync(It.Is<CashTransaction>(t =>
            t.CashShiftId == shiftId &&
            t.TransactionType == "CASH_DROP" &&
            t.Amount == 250.00m &&
            t.Notes == "Remesa" &&
            t.RecipientName == "Admin" &&
            t.CreatedBy == userId
        )), Times.Once);
        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task CloseShiftAsync_ShouldCalculateCorrectMath_AndUpdateStatusToClosed()
    {
        // Arrange
        var shiftId = 5L;
        var shift = new CashShift { Id = shiftId, Status = "OPEN", StartingCash = 100.00m };
        _cashShiftRepoMock.Setup(x => x.GetByIdAsync(shiftId)).ReturnsAsync(shift);

        _cashShiftRepoMock.Setup(x => x.GetCashSalesAmountAsync(shiftId)).ReturnsAsync(450.00m);
        _cashShiftRepoMock.Setup(x => x.GetManualTransactionsSumAsync(shiftId, "FLOAT_IN")).ReturnsAsync(50.00m);
        _cashShiftRepoMock.Setup(x => x.GetManualTransactionsSumAsync(shiftId, "CASH_DROP")).ReturnsAsync(300.00m);
        _cashShiftRepoMock.Setup(x => x.GetManualTransactionsSumAsync(shiftId, "EXPENSE")).ReturnsAsync(20.00m);
        _cashShiftRepoMock.Setup(x => x.UpdateAsync(It.IsAny<CashShift>())).ReturnsAsync(true);

        // Math:
        // Expected = StartingCash + CashSales + FloatIns - CashDrops - Expenses
        // Expected = 100.00 + 450.00 + 50.00 - 300.00 - 20.00 = 280.00
        // Actual = 280.00
        // Discrepancy = 280.00 - 280.00 = 0.00

        // Act
        var result = await _cashShiftService.CloseShiftAsync(shiftId, 280.00m, "Todo cuadrado");

        // Assert
        result.Should().BeTrue();
        shift.Status.Should().Be("CLOSED");
        shift.EndingCashExpected.Should().Be(280.00m);
        shift.EndingCashActual.Should().Be(280.00m);
        shift.Discrepancy.Should().Be(0.00m);
        shift.ClosingNotes.Should().Be("Todo cuadrado");

        _uowMock.Verify(x => x.BeginTransaction(), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }
}
