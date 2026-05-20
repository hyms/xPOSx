using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using XPos.Api.Controllers;
using XPos.Api.Dtos;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;
using BC = BCrypt.Net.BCrypt;

namespace XPos.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _configurationMock = new Mock<IConfiguration>();

        // Setup configuration for JWT
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns("super_secret_key_that_is_long_enough_for_sha256");
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("XPosApi");
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("XPosUsers");
        _configurationMock.Setup(x => x["Jwt:ExpireMinutes"]).Returns("1440");

        _authController = new AuthController(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _configurationMock.Object
        );
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var password = "password123";
        var hashedPassword = BC.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = hashedPassword,
            IsActive = true,
            RoleId = 1
        };

        var role = new Role
        {
            Id = 1,
            Name = "Admin",
            Permissions = new List<Permission>
            {
                new Permission { Name = "users_view" },
                new Permission { Name = "warehouses_all_access" }
            }
        };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("testuser")).ReturnsAsync(user);
        _roleRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _userRepositoryMock.Setup(x => x.GetUserWarehouseIdsAsync(1)).ReturnsAsync(new List<long> { 1, 2 });

        var loginDto = new LoginDto { Username = "testuser", Password = password };

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        var okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        var response = okResult.Value.As<AuthResponseDto>();
        response.Token.Should().NotBeNullOrEmpty();
        response.Username.Should().Be("testuser");
        response.Permissions.Should().Contain("users_view");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = BC.HashPassword("correct_password"),
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("testuser")).ReturnsAsync(user);

        var loginDto = new LoginDto { Username = "testuser", Password = "wrong_password" };

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = BC.HashPassword("password"),
            IsActive = false
        };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("testuser")).ReturnsAsync(user);

        var loginDto = new LoginDto { Username = "testuser", Password = "password" };

        // Act
        var result = await _authController.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task SwitchWarehouse_WithAccess_ReturnsNewToken()
    {
        // Arrange
        var user = new User { Id = 1, Username = "testuser", IsActive = true, RoleId = 1 };
        var role = new Role { Id = 1, Permissions = new List<Permission> { new Permission { Name = "warehouses_all_access" } } };
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _roleRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _userRepositoryMock.Setup(x => x.GetUserWarehouseIdsAsync(1)).ReturnsAsync(new List<long> { 1 });

        var claims = new List<Claim> { new Claim("id", "1") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _authController.SwitchWarehouse(1);

        // Assert
        var okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
    }
}
