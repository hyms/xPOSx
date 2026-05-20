using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;

namespace XPos.Tests;

public class ProductControllersTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly ProductsController _productsController;

    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly CategoriesController _categoriesController;

    public ProductControllersTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _productsController = new ProductsController(_productRepoMock.Object);

        _categoryRepoMock = new Mock<ICategoryRepository>();
        _categoriesController = new CategoriesController(_categoryRepoMock.Object);
    }

    [Fact]
    public async Task Products_FullCycle()
    {
        _productRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Product { Id = 1 });
        var getResult = await _productsController.GetById(1);
        getResult.Should().BeOfType<OkObjectResult>();

        _productRepoMock.Setup(x => x.CreateAsync(It.IsAny<Product>())).ReturnsAsync(1L);
        var createResult = await _productsController.Create(new Product());
        createResult.Should().BeOfType<CreatedAtActionResult>();

        var product = new Product { Id = 1 };
        _productRepoMock.Setup(x => x.UpdateAsync(product)).ReturnsAsync(true);
        var updateResult = await _productsController.Update(1, product);
        updateResult.Should().BeOfType<NoContentResult>();

        _productRepoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        var deleteResult = await _productsController.Delete(1);
        deleteResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Categories_FullCycle()
    {
        _categoryRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Category { Id = 1 });
        var getResult = await _categoriesController.GetById(1);
        getResult.Should().BeOfType<OkObjectResult>();

        _categoryRepoMock.Setup(x => x.CreateAsync(It.IsAny<Category>())).ReturnsAsync(1L);
        var createResult = await _categoriesController.Create(new Category());
        createResult.Should().BeOfType<CreatedAtActionResult>();

        var category = new Category { Id = 1 };
        _categoryRepoMock.Setup(x => x.UpdateAsync(category)).ReturnsAsync(true);
        var updateResult = await _categoriesController.Update(1, category);
        updateResult.Should().BeOfType<NoContentResult>();

        _categoryRepoMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        var deleteResult = await _categoriesController.Delete(1);
        deleteResult.Should().BeOfType<NoContentResult>();
    }
}
