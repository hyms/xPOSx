using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;

namespace XPos.Tests;

public class MiscControllersTests
{
    [Fact]
    public async Task UnitsController_FullCycle()
    {
        var repoMock = new Mock<IUnitRepository>();
        var controller = new UnitsController(repoMock.Object);

        repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Unit>());
        (await controller.GetAll()).Should().BeOfType<OkObjectResult>();

        repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Unit { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        repoMock.Setup(x => x.CreateAsync(It.IsAny<Unit>())).ReturnsAsync(1L);
        (await controller.Create(new Unit())).Should().BeOfType<CreatedAtActionResult>();

        repoMock.Setup(x => x.UpdateAsync(It.IsAny<Unit>())).ReturnsAsync(true);
        (await controller.Update(1, new Unit { Id = 1 })).Should().BeOfType<NoContentResult>();

        repoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ReportsController_AllMethods()
    {
        var serviceMock = new Mock<IReportService>();
        var controller = new ReportsController(serviceMock.Object);

        (await controller.GetSalesReport(null, null, null)).Should().BeOfType<OkObjectResult>();
        (await controller.GetPurchaseReport(null, null, null)).Should().BeOfType<OkObjectResult>();
        (await controller.GetStockReport(null)).Should().BeOfType<OkObjectResult>();
        (await controller.GetProfitLossReport(null, null)).Should().BeOfType<OkObjectResult>();
        (await controller.GetDashboardSummary()).Should().BeOfType<OkObjectResult>();
        (await controller.GetClientReport()).Should().BeOfType<OkObjectResult>();
        (await controller.GetProviderReport()).Should().BeOfType<OkObjectResult>();
        (await controller.GetTopProducts(10)).Should().BeOfType<OkObjectResult>();
        (await controller.GetProductMovements(1)).Should().BeOfType<OkObjectResult>();
        (await controller.GetStockAlerts()).Should().BeOfType<OkObjectResult>();
        (await controller.GetActivityReport(null, null, null)).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PaymentGatewaySettingsController_FullCycle()
    {
        var serviceMock = new Mock<IPaymentGatewaySettingsService>();
        var controller = new PaymentGatewaySettingsController(serviceMock.Object);

        serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PaymentGatewaySettings>());
        (await controller.GetAll()).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new PaymentGatewaySettings { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreateAsync(It.IsAny<PaymentGatewaySettings>())).ReturnsAsync(1L);
        (await controller.Create(new PaymentGatewaySettings())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.UpdateAsync(It.IsAny<PaymentGatewaySettings>())).ReturnsAsync(true);
        (await controller.Update(1, new PaymentGatewaySettings { Id = 1 })).Should().BeOfType<NoContentResult>();

        serviceMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task SmsSettingsController_FullCycle()
    {
        var serviceMock = new Mock<ISmsSettingsService>();
        var controller = new SmsSettingsController(serviceMock.Object);

        serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<SmsSettings>());
        (await controller.GetAll()).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new SmsSettings { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreateAsync(It.IsAny<SmsSettings>())).ReturnsAsync(1L);
        (await controller.Create(new SmsSettings())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.UpdateAsync(It.IsAny<SmsSettings>())).ReturnsAsync(true);
        (await controller.Update(1, new SmsSettings { Id = 1 })).Should().BeOfType<NoContentResult>();

        serviceMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task SmsSettingsApiController_FullCycle()
    {
        var serviceMock = new Mock<ISmsSettingsService>();
        var controller = new SmsSettingsApiController(serviceMock.Object);

        serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<SmsSettings>());
        (await controller.Get()).Should().BeOfType<OkObjectResult>();

        (await controller.CreateOrUpdate(new SmsSettings())).Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PaymentGatewaySettingsApiController_FullCycle()
    {
        var serviceMock = new Mock<IPaymentGatewaySettingsService>();
        var controller = new PaymentGatewaySettingsApiController(serviceMock.Object);

        serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<PaymentGatewaySettings>());
        (await controller.Get()).Should().BeOfType<OkObjectResult>();

        (await controller.CreateOrUpdate(new PaymentGatewaySettings())).Should().BeOfType<OkObjectResult>();
    }
}
