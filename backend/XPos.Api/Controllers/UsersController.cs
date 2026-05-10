using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound();
        
        user.Password = string.Empty;
        return Ok(user);
    }

    [Authorize(Policy = "users_view")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users);
    }

    [Authorize(Policy = "users_view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [Authorize(Policy = "users_create")]
    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        var id = await _userRepository.CreateAsync(user);
        user.Id = id;
        return CreatedAtAction(nameof(GetById), new { id }, user);
    }

    [Authorize(Policy = "users_edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, User user)
    {
        if (id != user.Id) return BadRequest();
        var success = await _userRepository.UpdateAsync(user);
        if (!success) return NotFound();
        return NoContent();
    }

    [Authorize(Policy = "users_delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await _userRepository.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [Authorize(Policy = "users_edit")]
    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(long id)
    {
        var success = await _userRepository.ToggleUserStatusAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }


    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(User userDto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound();

        // Only update allowed fields
        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.Email = userDto.Email;
        user.Username = userDto.Username;

        var success = await _userRepository.UpdateAsync(user);
        return success ? Ok(user) : BadRequest();
    }

    [HttpPut("profile/password")]
    public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();

        if (string.IsNullOrEmpty(dto.NewPassword)) return BadRequest("Password is required");

        var success = await _userRepository.UpdatePasswordAsync(userId, dto.NewPassword);
        return success ? NoContent() : BadRequest();
    }
}
