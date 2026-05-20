using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;

namespace XPos.Tests;

public class WarehousesControllerTests
{
    private readonly Mock<IWarehouseRepository> _repoMock;
    private readonly WarehousesController _controller;

    public WarehousesControllerTests()
    {
        _repoMock = new Mock<IWarehouseRepository>();
        _controller = new WarehousesController(_repoMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Warehouse>());
        var result = await _controller.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Warehouse { Id = 1 });
        var result = await _controller.GetById(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        _repoMock.Setup(x => x.CreateAsync(It.IsAny<Warehouse>())).ReturnsAsync(1L);
        var result = await _controller.Create(new Warehouse());
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        var w = new Warehouse { Id = 1 };
        _repoMock.Setup(x => x.UpdateAsync(w)).ReturnsAsync(true);
        var result = await _controller.Update(1, w);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        _repoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        var result = await _controller.Delete(1);
        result.Should().BeOfType<NoContentResult>();
    }
}
