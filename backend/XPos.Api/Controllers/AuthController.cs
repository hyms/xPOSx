using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using XPos.Api.Dtos;
using XPos.Domain.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace XPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var user = await _userRepository.GetByUsernameAsync(login.Username);
        
        if (user == null || !user.IsActive || !BC.Verify(login.Password, user.Password))
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        var role = await _roleRepository.GetByIdAsync(user.RoleId ?? 0);
        var permissions = role?.Permissions.Select(p => p.Name) ?? Enumerable.Empty<string>();

        var warehouseIds = await _userRepository.GetUserWarehouseIdsAsync(user.Id);
        var activeWarehouseId = user.DefaultWarehouseId ?? warehouseIds.FirstOrDefault();
        var hasAllAccess = permissions.Contains("warehouses_all_access");

        var token = GenerateJwtToken(user, permissions, warehouseIds, activeWarehouseId, hasAllAccess);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Permissions = permissions,
            ActiveWarehouseId = activeWarehouseId
        });
    }

    [HttpPost("switch-warehouse/{warehouseId}")]
    public async Task<IActionResult> SwitchWarehouse(long warehouseId)
    {
        var userIdString = User.FindFirst("id")?.Value;
        if (!long.TryParse(userIdString, out var userId)) return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive) return Unauthorized();

        var role = await _roleRepository.GetByIdAsync(user.RoleId ?? 0);
        var permissions = role?.Permissions.Select(p => p.Name) ?? Enumerable.Empty<string>();
        
        bool hasAccess = permissions.Contains("warehouses_all_access");
        if (!hasAccess)
        {
            var allowedWarehouses = await _userRepository.GetUserWarehouseIdsAsync(userId);
            hasAccess = allowedWarehouses.Contains(warehouseId);
        }

        if (!hasAccess) return Forbid("No tiene acceso a este almacén.");
        
        var warehouseIds = await _userRepository.GetUserWarehouseIdsAsync(userId);

        var token = GenerateJwtToken(user, permissions, warehouseIds, warehouseId, hasAccess);

        return Ok(new { token });
    }

    private string GenerateJwtToken(XPos.Domain.Models.User user, IEnumerable<string> permissions, IEnumerable<long> warehouseIds, long? activeWarehouseId, bool hasAllAccess)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, (user.RoleId ?? 0).ToString()),
            new Claim("has_all_warehouses_access", hasAllAccess.ToString().ToLower()),
            new Claim("allowed_warehouses", string.Join(",", warehouseIds)),
            new Claim("active_warehouse_id", activeWarehouseId?.ToString() ?? "")
        };

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "1440")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
