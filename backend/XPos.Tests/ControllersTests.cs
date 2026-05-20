using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace XPos.Tests;

public class ControllersTests
{
    [Fact]
    public async Task WarehousesController_GetAll_ReturnsOk()
    {
        var repoMock = new Mock<IWarehouseRepository>();
        repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Warehouse>());
        var controller = new WarehousesController(repoMock.Object);
        var result = await controller.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ExpensesController_FullCycle()
    {
        var repoMock = new Mock<IExpenseRepository>();
        var controller = new ExpensesController(repoMock.Object);
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };

        repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Expense { Id = 1 });
        var getResult = await controller.GetById(1);
        getResult.Should().BeOfType<OkObjectResult>();

        repoMock.Setup(x => x.CreateAsync(It.IsAny<Expense>())).ReturnsAsync(1L);
        var createResult = await controller.Create(new Expense());
        createResult.Should().BeOfType<CreatedAtActionResult>();

        var expense = new Expense { Id = 1 };
        repoMock.Setup(x => x.UpdateAsync(expense)).ReturnsAsync(true);
        var updateResult = await controller.Update(1, expense);
        updateResult.Should().BeOfType<NoContentResult>();

        repoMock.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(true);
        var deleteResult = await controller.Delete(1);
        deleteResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RolesController_GetAll_ReturnsOk()
    {
        var repoMock = new Mock<IRoleRepository>();
        repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Role>());
        var controller = new RolesController(repoMock.Object);
        var result = await controller.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ClientsController_FullCycle()
    {
        var repoMock = new Mock<IClientRepository>();
        var controller = new ClientsController(repoMock.Object);

        // GetById
        repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Client { Id = 1 });
        var getResult = await controller.GetById(1);
        getResult.Should().BeOfType<OkObjectResult>();

        // Create
        repoMock.Setup(x => x.CreateAsync(It.IsAny<Client>())).ReturnsAsync(1L);
        var createResult = await controller.Create(new Client());
        createResult.Should().BeOfType<CreatedAtActionResult>();

        // Update
        var client = new Client { Id = 1 };
        repoMock.Setup(x => x.UpdateAsync(client)).ReturnsAsync(true);
        var updateResult = await controller.Update(1, client);
        updateResult.Should().BeOfType<NoContentResult>();

        // Delete
        repoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        var deleteResult = await controller.Delete(1);
        deleteResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ProvidersController_FullCycle()
    {
        var repoMock = new Mock<IProviderRepository>();
        var controller = new ProvidersController(repoMock.Object);

        repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Provider { Id = 1 });
        var getResult = await controller.GetById(1);
        getResult.Should().BeOfType<OkObjectResult>();

        repoMock.Setup(x => x.CreateAsync(It.IsAny<Provider>())).ReturnsAsync(1L);
        var createResult = await controller.Create(new Provider());
        createResult.Should().BeOfType<CreatedAtActionResult>();

        var provider = new Provider { Id = 1 };
        repoMock.Setup(x => x.UpdateAsync(provider)).ReturnsAsync(true);
        var updateResult = await controller.Update(1, provider);
        updateResult.Should().BeOfType<NoContentResult>();

        repoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        var deleteResult = await controller.Delete(1);
        deleteResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task PermissionsController_GetAll_ReturnsOk()
    {
        var repoMock = new Mock<IPermissionRepository>();
        repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Permission>());
        var controller = new PermissionsController(repoMock.Object);
        var result = await controller.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }
}
