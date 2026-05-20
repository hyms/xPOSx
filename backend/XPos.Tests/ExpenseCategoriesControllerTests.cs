using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace XPos.Tests;

public class ExpenseCategoriesControllerTests
{
    private readonly Mock<IExpenseCategoryRepository> _repoMock;
    private readonly ExpenseCategoriesController _controller;

    public ExpenseCategoriesControllerTests()
    {
        _repoMock = new Mock<IExpenseCategoryRepository>();
        _controller = new ExpenseCategoriesController(_repoMock.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<ExpenseCategory>());
        var result = await _controller.GetAll();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new ExpenseCategory { Id = 1 });
        var result = await _controller.GetById(1);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        _repoMock.Setup(x => x.CreateAsync(It.IsAny<ExpenseCategory>())).ReturnsAsync(1L);
        var result = await _controller.Create(new ExpenseCategory());
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        var ec = new ExpenseCategory { Id = 1 };
        _repoMock.Setup(x => x.UpdateAsync(ec)).ReturnsAsync(true);
        var result = await _controller.Update(1, ec);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        _repoMock.Setup(x => x.DeleteAsync(1, 1)).ReturnsAsync(true);
        var result = await _controller.Delete(1);
        result.Should().BeOfType<NoContentResult>();
    }
}
