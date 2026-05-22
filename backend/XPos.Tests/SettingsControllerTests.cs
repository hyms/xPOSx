using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;

namespace XPos.Tests;

public class SettingsControllerTests
{
    private readonly Mock<ISettingRepository> _repoMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _repoMock = new Mock<ISettingRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _envMock = new Mock<IWebHostEnvironment>();

        // Set up IMemoryCache Mock behavior for TryGetValue
        object? outValue = null;
        _cacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        _controller = new SettingsController(_repoMock.Object, _cacheMock.Object, _envMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenExists()
    {
        _repoMock.Setup(x => x.GetAsync()).ReturnsAsync(new Setting());
        var result = await _controller.Get();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        var setting = new Setting { Id = 1 };
        _repoMock.Setup(x => x.UpdateAsync(setting)).ReturnsAsync(true);
        var result = await _controller.Update(1, setting);
        result.Should().BeOfType<NoContentResult>();
    }
}
