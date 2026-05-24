using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace XPos.Tests;

public class OnlineSalesControllerTests
{
    private readonly Mock<ISaleService> _saleServiceMock;
    private readonly Mock<ICmsRepository> _cmsRepoMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly OnlineSalesController _controller;

    public OnlineSalesControllerTests()
    {
        _saleServiceMock = new Mock<ISaleService>();
        _cmsRepoMock = new Mock<ICmsRepository>();
        _environmentMock = new Mock<IWebHostEnvironment>();

        _controller = new OnlineSalesController(
            _saleServiceMock.Object,
            _cmsRepoMock.Object,
            _environmentMock.Object
        );
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
    public async Task GetById_ReturnsOk_WhenExists()
    {
        // Arrange
        var sale = new Sale { Id = 1, Ref = "WEB-001" };
        _saleServiceMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(sale);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        okResult.Value.As<Sale>().Ref.Should().Be("WEB-001");
    }
}
