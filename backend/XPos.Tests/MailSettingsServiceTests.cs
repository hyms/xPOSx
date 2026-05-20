using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using FluentAssertions;

namespace XPos.Tests;

public class MailSettingsServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMailSettingsRepository> _repoMock;
    private readonly MailSettingsService _service;

    public MailSettingsServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repoMock = new Mock<IMailSettingsRepository>();
        _service = new MailSettingsService(_uowMock.Object, _repoMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ShouldCreate_WhenNotExists()
    {
        _repoMock.Setup(x => x.GetAsync()).ReturnsAsync((MailSettings?)null);
        _repoMock.Setup(x => x.CreateAsync(It.IsAny<MailSettings>())).ReturnsAsync(1L);

        var result = await _service.CreateOrUpdateAsync(new MailSettings());

        result.Should().Be(1L);
        _repoMock.Verify(x => x.CreateAsync(It.IsAny<MailSettings>()), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ShouldUpdate_WhenExists()
    {
        _repoMock.Setup(x => x.GetAsync()).ReturnsAsync(new MailSettings { Id = 5 });
        
        var result = await _service.CreateOrUpdateAsync(new MailSettings());

        result.Should().Be(5L);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<MailSettings>()), Times.Once);
        _uowMock.Verify(x => x.Commit(), Times.Once);
    }
}
