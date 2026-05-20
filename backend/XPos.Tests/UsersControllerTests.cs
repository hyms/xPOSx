using Microsoft.AspNetCore.Mvc;
using Moq;
using XPos.Api.Controllers;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace XPos.Tests;

public class UsersControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UsersController _usersController;

    public UsersControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _usersController = new UsersController(_userRepositoryMock.Object);

        // Setup a default user context
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _usersController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetProfile_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var user = new User { Id = 1, Username = "testuser" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _usersController.GetProfile();

        // Assert
        var okResult = result.As<OkObjectResult>();
        okResult.Should().NotBeNull();
        var returnedUser = okResult.Value.As<User>();
        returnedUser.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithUsers()
    {
        // Arrange
        var users = new List<User> { new User { Id = 1 }, new User { Id = 2 } };
        _userRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _usersController.GetAll();

        // Assert
        var okResult = result.As<OkObjectResult>();
        okResult.Value.As<IEnumerable<User>>().Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithNewUser()
    {
        // Arrange
        var newUser = new User { Username = "newuser", Password = "password" };
        _userRepositoryMock.Setup(x => x.CreateAsync(newUser)).ReturnsAsync(10);

        // Act
        var result = await _usersController.Create(newUser);

        // Assert
        var createdResult = result.As<CreatedAtActionResult>();
        createdResult.Should().NotBeNull();
        createdResult!.ActionName.Should().Be(nameof(UsersController.GetById));
        createdResult.RouteValues!["id"].Should().Be(10L);
        newUser.Id.Should().Be(10);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var user = new User { Id = 1, Username = "updateduser" };
        _userRepositoryMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(true);

        // Act
        var result = await _usersController.Update(1, user);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var user = new User { Id = 2 };

        // Act
        var result = await _usersController.Update(1, user);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _usersController.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ToggleStatus_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.ToggleUserStatusAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _usersController.ToggleStatus(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UpdateProfile_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var user = new User { Id = 1, Username = "old" };
        var updateDto = new User { Username = "new", Email = "new@ex.com" };
        _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(true);

        // Act
        var result = await _usersController.UpdateProfile(updateDto);

        // Assert
        var okResult = result.As<OkObjectResult>();
        var returnedUser = okResult.Value.As<User>();
        returnedUser.Username.Should().Be("new");
    }

    [Fact]
    public async Task UpdatePassword_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var dto = new PasswordUpdateDto { NewPassword = "newpassword" };
        _userRepositoryMock.Setup(x => x.UpdatePasswordAsync(1, "newpassword")).ReturnsAsync(true);

        // Act
        var result = await _usersController.UpdatePassword(dto);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
