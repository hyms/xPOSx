using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Dtos;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace XPos.Tests;

public class MoreControllersTests
{
    private ControllerContext CreateContext()
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }

    [Fact]
    public async Task AdjustmentsController_FullCycle()
    {
        var serviceMock = new Mock<IAdjustmentService>();
        var controller = new AdjustmentsController(serviceMock.Object) { ControllerContext = CreateContext() };

        serviceMock.Setup(x => x.GetAllAsync(It.IsAny<string>())).ReturnsAsync(new List<AdjustmentReadDto>());
        (await controller.GetAll(null)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Adjustment { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreateAdjustmentAsync(It.IsAny<CreateAdjustmentDto>(), 1)).ReturnsAsync(1L);
        (await controller.Create(new CreateAdjustmentDto())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.UpdateAdjustmentAsync(It.IsAny<UpdateAdjustmentDto>(), 1)).ReturnsAsync(true);
        (await controller.Update(1, new UpdateAdjustmentDto { Id = 1 })).Should().BeOfType<OkResult>();

        serviceMock.Setup(x => x.DeleteAdjustmentAsync(1, 1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task PurchasesController_FullCycle()
    {
        var serviceMock = new Mock<IPurchaseService>();
        var controller = new PurchasesController(serviceMock.Object) { ControllerContext = CreateContext() };

        serviceMock.Setup(x => x.GetAllAsync(It.IsAny<PagingParams>())).ReturnsAsync(new PagedResult<PurchaseReadDto>());
        (await controller.GetAll(new PagingParams())).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Purchase { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreatePurchaseAsync(It.IsAny<CreatePurchaseDto>(), 1)).ReturnsAsync(1L);
        (await controller.Create(new CreatePurchaseDto())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.UpdatePurchaseAsync(It.IsAny<UpdatePurchaseDto>(), 1)).ReturnsAsync(true);
        (await controller.Update(1, new UpdatePurchaseDto { Id = 1 })).Should().BeOfType<NoContentResult>();

        serviceMock.Setup(x => x.DeletePurchaseAsync(1, 1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task QuotationsController_FullCycle()
    {
        var serviceMock = new Mock<IQuotationService>();
        var controller = new QuotationsController(serviceMock.Object) { ControllerContext = CreateContext() };

        serviceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<QuotationReadDto>());
        (await controller.GetAll()).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Quotation { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreateAsync(It.IsAny<CreateQuotationDto>(), 1)).ReturnsAsync(1L);
        (await controller.Create(new CreateQuotationDto())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.UpdateAsync(It.IsAny<UpdateQuotationDto>(), 1)).ReturnsAsync(true);
        (await controller.Update(1, new UpdateQuotationDto { Id = 1 })).Should().BeOfType<OkResult>();

        serviceMock.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task TransfersController_FullCycle()
    {
        var serviceMock = new Mock<ITransferService>();
        var controller = new TransfersController(serviceMock.Object) { ControllerContext = CreateContext() };

        serviceMock.Setup(x => x.GetAllAsync(It.IsAny<string>())).ReturnsAsync(new List<TransferReadDto>());
        (await controller.GetAll(null)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Transfer { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreateTransferAsync(It.IsAny<CreateTransferDto>(), 1)).ReturnsAsync(1L);
        (await controller.Create(new CreateTransferDto())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.UpdateTransferAsync(It.IsAny<UpdateTransferDto>(), 1)).ReturnsAsync(true);
        (await controller.Update(1, new UpdateTransferDto { Id = 1 })).Should().BeOfType<OkResult>();

        serviceMock.Setup(x => x.DeleteTransferAsync(1, 1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task SaleReturnsController_FullCycle()
    {
        var serviceMock = new Mock<IReturnService>();
        var controller = new SaleReturnsController(serviceMock.Object) { ControllerContext = CreateContext() };

        serviceMock.Setup(x => x.GetAllSaleReturnsAsync()).ReturnsAsync(new List<SaleReturnReadDto>());
        (await controller.GetAll()).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetSaleReturnByIdAsync(1)).ReturnsAsync(new SaleReturn { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreateSaleReturnAsync(It.IsAny<CreateSaleReturnDto>(), 1)).ReturnsAsync(1L);
        (await controller.Create(new CreateSaleReturnDto())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.DeleteSaleReturnAsync(1, 1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task PurchaseReturnsController_FullCycle()
    {
        var serviceMock = new Mock<IReturnService>();
        var controller = new PurchaseReturnsController(serviceMock.Object) { ControllerContext = CreateContext() };

        serviceMock.Setup(x => x.GetAllPurchaseReturnsAsync()).ReturnsAsync(new List<PurchaseReturnReadDto>());
        (await controller.GetAll()).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.GetPurchaseReturnByIdAsync(1)).ReturnsAsync(new PurchaseReturn { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        serviceMock.Setup(x => x.CreatePurchaseReturnAsync(It.IsAny<CreatePurchaseReturnDto>(), 1)).ReturnsAsync(1L);
        (await controller.Create(new CreatePurchaseReturnDto())).Should().BeOfType<CreatedAtActionResult>();

        serviceMock.Setup(x => x.DeletePurchaseReturnAsync(1, 1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ReportsController_DashboardSummary_ReturnsOk()
    {
        var serviceMock = new Mock<IReportService>();
        var controller = new ReportsController(serviceMock.Object);
        var result = await controller.GetDashboardSummary();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void NotificationsController_GetSettings_ReturnsOk()
    {
        var controller = new NotificationsController();
        var result = controller.GetSettings();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CashRegistersController_FullCycle()
    {
        var repoMock = new Mock<ICashRegisterRepository>();
        var controller = new CashRegistersController(repoMock.Object) { ControllerContext = CreateContext() };

        repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<CashRegister>());
        (await controller.GetAll()).Should().BeOfType<OkObjectResult>();

        repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new CashRegister { Id = 1 });
        (await controller.GetById(1)).Should().BeOfType<OkObjectResult>();

        repoMock.Setup(x => x.CreateAsync(It.IsAny<CashRegister>())).ReturnsAsync(1L);
        (await controller.Create(new CashRegister { Name = "Caja Test" })).Should().BeOfType<CreatedAtActionResult>();

        repoMock.Setup(x => x.UpdateAsync(It.IsAny<CashRegister>())).ReturnsAsync(true);
        (await controller.Update(1, new CashRegister { Id = 1, Name = "Caja Test" })).Should().BeOfType<OkObjectResult>();

        repoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        (await controller.Delete(1)).Should().BeOfType<OkObjectResult>();
    }
}
