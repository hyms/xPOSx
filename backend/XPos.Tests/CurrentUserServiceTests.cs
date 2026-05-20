using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using XPos.Api.Services;
using FluentAssertions;

namespace XPos.Tests;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentUserService _service;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _service = new CurrentUserService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void UserId_ShouldReturnCorrectValue()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("id", "123") }));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _service.UserId.Should().Be(123);
    }

    [Fact]
    public void ActiveWarehouseId_ShouldReturnCorrectValue()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("active_warehouse_id", "5") }));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _service.ActiveWarehouseId.Should().Be(5);
    }

    [Fact]
    public void HasAllWarehousesAccess_ShouldReturnCorrectValue()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("has_all_warehouses_access", "true") }));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _service.HasAllWarehousesAccess.Should().BeTrue();
    }

    [Fact]
    public void AllowedWarehouseIds_ShouldReturnCorrectList()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("allowed_warehouses", "1,2,3") }));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _service.AllowedWarehouseIds.Should().BeEquivalentTo(new List<long> { 1, 2, 3 });
    }
}
