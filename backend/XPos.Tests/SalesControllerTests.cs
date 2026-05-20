using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace XPos.Tests;

public class SalesControllerTests
{
    private readonly Mock<ISaleService> _saleServiceMock;
    private readonly SalesController _controller;

    public SalesControllerTests()
    {
        _saleServiceMock = new Mock<ISaleService>();
        _controller = new SalesController(_saleServiceMock.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var paging = new PagingParams();
        var pagedResult = new PagedResult<SaleReadDto>();
        _saleServiceMock.Setup(x => x.GetAllAsync(paging)).ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(paging);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        // Arrange
        _saleServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Sale { Id = 1 });

        // Act
        var result = await _controller.GetById(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        _saleServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Sale?)null);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        // Arrange
        var sale = new Sale { Id = 0 };
        _saleServiceMock.Setup(x => x.CreateSaleAsync(sale, 1)).ReturnsAsync(100L);

        // Act
        var result = await _controller.Create(sale);

        // Assert
        var createdResult = result.As<CreatedAtActionResult>();
        createdResult.Should().NotBeNull();
        createdResult!.RouteValues!["id"].Should().Be(100L);
    }
}
