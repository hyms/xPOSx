using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;

namespace XPos.Tests;

public class SettingsControllerTests
{
    private readonly Mock<ISettingRepository> _repoMock;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _repoMock = new Mock<ISettingRepository>();
        _controller = new SettingsController(_repoMock.Object);
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
