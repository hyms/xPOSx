using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using XPos.Api.Authorization;
using FluentAssertions;

namespace XPos.Tests;

public class PermissionHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenUserHasPermission()
    {
        // Arrange
        var handler = new PermissionHandler();
        var requirement = new PermissionRequirement("test_permission");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permission", "test_permission")
        }));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldNotSucceed_WhenUserDoesNotHavePermission()
    {
        // Arrange
        var handler = new PermissionHandler();
        var requirement = new PermissionRequirement("test_permission");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permission", "other_permission")
        }));
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
