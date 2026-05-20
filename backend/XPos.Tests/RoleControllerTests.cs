using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;

namespace XPos.Tests;

public class RoleControllerTests
{
    private readonly Mock<IRoleRepository> _repoMock;
    private readonly RolesController _controller;

    public RoleControllerTests()
    {
        _repoMock = new Mock<IRoleRepository>();
        _controller = new RolesController(_repoMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Role>());
        var result = await _controller.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Role { Id = 1 });
        var result = await _controller.GetById(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        _repoMock.Setup(x => x.CreateAsync(It.IsAny<Role>())).ReturnsAsync(1L);
        var result = await _controller.Create(new Role());
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        var role = new Role { Id = 1 };
        _repoMock.Setup(x => x.UpdateAsync(role)).ReturnsAsync(true);
        var result = await _controller.Update(1, role);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        _repoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        var result = await _controller.Delete(1);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task AssignPermissions_ReturnsNoContent()
    {
        _repoMock.Setup(x => x.AssignPermissionsAsync(1, It.IsAny<IEnumerable<long>>())).ReturnsAsync(true);
        var result = await _controller.AssignPermissions(1, new List<long> { 1, 2 });
        result.Should().BeOfType<NoContentResult>();
    }
}
