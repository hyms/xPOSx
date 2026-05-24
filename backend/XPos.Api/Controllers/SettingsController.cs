using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingRepository _settingRepository;
    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _environment;
    private const string CacheKey = "system_settings";

    public SettingsController(
        ISettingRepository settingRepository, 
        IMemoryCache cache,
        IWebHostEnvironment environment)
    {
        _settingRepository = settingRepository;
        _cache = cache;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (!_cache.TryGetValue(CacheKey, out SettingReadDto? dto))
        {
            var settings = await _settingRepository.GetAsync();
            if (settings == null) return NotFound();

            dto = new SettingReadDto
            {
                Id = settings.Id,
                CompanyName = settings.CompanyName,
                Email = settings.Email,
                CompanyPhone = settings.CompanyPhone,
                CompanyAddress = settings.CompanyAddress,
                Logo = settings.Logo,
                Favicon = settings.Favicon,
                Version = settings.Version,
                SettingsVersion = settings.SettingsVersion,
                Days = settings.Days,
                CurrencyCode = settings.CurrencyCode,
                CurrencySymbol = settings.CurrencySymbol,
                CurrencyName = settings.CurrencyName,
                SiatEnvironment = settings.SiatEnvironment,
                SiatModality = settings.SiatModality,
                SiatEmissionType = settings.SiatEmissionType,
                QrCodePath = settings.QrCodePath
            };

            _cache.Set(CacheKey, dto, TimeSpan.FromHours(24));
        }

        // Cache Invalidation Header
        Response.Headers.Append("X-Settings-Version", dto!.SettingsVersion.ToString());

        return Ok(dto);
    }

    [HttpPost("upload-media")]
    public async Task<IActionResult> UploadMedia([FromForm] IFormFile file, [FromForm] string type)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");
        
        type = type.ToLower();
        if (type != "logo" && type != "favicon") return BadRequest("Invalid type");

        // Security Validations
        var allowedExtensions = type == "logo" 
            ? new[] { ".png", ".jpg", ".jpeg" } 
            : new[] { ".png", ".ico" };
        
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension)) return BadRequest("Invalid file extension");

        var maxSize = type == "logo" ? 2 * 1024 * 1024 : 500 * 1024;
        if (file.Length > maxSize) return BadRequest("File size exceeds limit");

        var allowedMimeTypes = type == "logo"
            ? new[] { "image/png", "image/jpeg" }
            : new[] { "image/png", "image/x-icon", "image/vnd.microsoft.icon" };
        
        if (!allowedMimeTypes.Contains(file.ContentType)) return BadRequest("Invalid MIME type");

        // Save File
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "identity");
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);
        var relativePath = $"/uploads/identity/{fileName}";

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update DB
        var success = await _settingRepository.UpdateMediaAsync(type, relativePath);
        if (!success) return StatusCode(500, "Error updating database");

        // Invalidate Cache
        _cache.Remove(CacheKey);

        // Get fresh settings to get the new version
        var settings = await _settingRepository.GetAsync();
        var newVersion = settings?.SettingsVersion ?? 0;
        Response.Headers.Append("X-Settings-Version", newVersion.ToString());

        return Ok(new { path = relativePath, settingsVersion = newVersion });
    }

    // Define the DTO within the file or move to Dtos namespace if preferred
    public class SettingReadDto
    {
        public long Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyAddress { get; set; }
        public string? Logo { get; set; }
        public string? Favicon { get; set; }
        public string Version { get; set; } = string.Empty;
        public int SettingsVersion { get; set; }
        public int Days { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? CurrencyName { get; set; }
        public int? SiatEnvironment { get; set; }
        public int? SiatModality { get; set; }
        public int? SiatEmissionType { get; set; }
        public string? QrCodePath { get; set; }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Setting setting)
    {
        if (id != setting.Id) return BadRequest();
        var success = await _settingRepository.UpdateAsync(setting);
        if (!success) return NotFound();
        
        _cache.Remove(CacheKey);
        return NoContent();
    }
}
