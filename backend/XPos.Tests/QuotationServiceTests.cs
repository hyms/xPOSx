using Moq;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using XPos.Domain.Services;
using XPos.Domain.Dtos;
using FluentAssertions;

namespace XPos.Tests;

public class QuotationServiceTests
{
    private readonly Mock<IQuotationRepository> _repoMock;
    private readonly QuotationService _service;

    public QuotationServiceTests()
    {
        _repoMock = new Mock<IQuotationRepository>();
        _service = new QuotationService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCalculateGrandTotalAndCallRepo()
    {
        // Arrange
        var userId = 1L;
        var dto = new CreateQuotationDto
        {
            Date = DateTime.Now,
            Details = new List<CreateQuotationDetailDto>
            {
                new CreateQuotationDetailDto { ProductId = 1, Quantity = 2, Price = 100 }
            },
            Discount = 10,
            Shipping = 5
        };

        _repoMock.Setup(x => x.CreateAsync(It.IsAny<Quotation>())).ReturnsAsync(700L);

        // Act
        var result = await _service.CreateAsync(dto, userId);

        // Assert
        result.Should().Be(700L);
        _repoMock.Verify(x => x.CreateAsync(It.Is<Quotation>(q => q.GrandTotal == 195)), Times.Once); // (2*100) - 10 + 5 = 195
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallRepo()
    {
        var dto = new UpdateQuotationDto { Id = 1, Details = new List<CreateQuotationDetailDto>() };
        await _service.UpdateAsync(dto, 1);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<Quotation>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepo()
    {
        await _service.DeleteAsync(1, 1);
        _repoMock.Verify(x => x.DeleteAsync(1, 1), Times.Once);
    }
}
