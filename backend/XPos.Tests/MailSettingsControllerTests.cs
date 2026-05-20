using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;

namespace XPos.Tests;

public class MailSettingsControllerTests
{
    private readonly Mock<IMailSettingsService> _serviceMock;
    private readonly MailSettingsController _controller;

    public MailSettingsControllerTests()
    {
        _serviceMock = new Mock<IMailSettingsService>();
        _controller = new MailSettingsController(_serviceMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsOk()
    {
        _serviceMock.Setup(x => x.GetAsync()).ReturnsAsync(new MailSettings());
        var result = await _controller.Get();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateOrUpdate_ReturnsOk()
    {
        _serviceMock.Setup(x => x.CreateOrUpdateAsync(It.IsAny<MailSettings>())).ReturnsAsync(1L);
        var result = await _controller.CreateOrUpdate(new MailSettings());
        result.Should().BeOfType<OkObjectResult>();
    }
}
