using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;

    public RolesController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleRepository.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return NotFound();
        return Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Role role)
    {
        var id = await _roleRepository.CreateAsync(role);
        role.Id = id;
        return CreatedAtAction(nameof(GetById), new { id }, role);
    }

    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> AssignPermissions(long id, [FromBody] List<long> permissionIds)
    {
        var success = await _roleRepository.AssignPermissionsAsync(id, permissionIds);
        if (!success) return BadRequest("Could not assign permissions.");
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Role role)
    {
        if (id != role.Id) return BadRequest();
        var success = await _roleRepository.UpdateAsync(role);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var success = await _roleRepository.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
