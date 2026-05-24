using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Ganss.Xss;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/cms/pages")]
public class CmsController : ControllerBase
{
    private readonly ICmsRepository _cmsRepository;

    public CmsController(ICmsRepository cmsRepository)
    {
        _cmsRepository = cmsRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pages = await _cmsRepository.GetAllAsync();
        return Ok(pages);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var page = await _cmsRepository.GetByIdAsync(id);
        return page == null ? NotFound() : Ok(page);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CmsPageInputDto input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Slug))
        {
            return BadRequest(new { message = "Título y slug son obligatorios." });
        }

        // Clean HTML content to prevent XSS strictly using Ganss.HtmlSanitizer
        var sanitizer = new HtmlSanitizer();
        var safeContent = sanitizer.Sanitize(input.ContentHtml);

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("id")?.Value;
        long? userId = long.TryParse(userIdStr, out var parsedId) ? parsedId : null;

        try
        {
            var page = new CmsPage
            {
                Title = input.Title,
                Slug = input.Slug,
                Content = safeContent,
                CreatedBy = userId
            };

            var id = await _cmsRepository.CreateAsync(page);

            return CreatedAtAction(nameof(GetById), new { id }, new { id, title = input.Title, slug = input.Slug, content = safeContent });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error al crear la página del CMS: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] CmsPageInputDto input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Slug))
        {
            return BadRequest(new { message = "Título y slug son obligatorios." });
        }

        // Clean HTML content to prevent XSS strictly using Ganss.HtmlSanitizer
        var sanitizer = new HtmlSanitizer();
        var safeContent = sanitizer.Sanitize(input.ContentHtml);

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("id")?.Value;
        long? userId = long.TryParse(userIdStr, out var parsedId) ? parsedId : null;

        try
        {
            var page = new CmsPage
            {
                Id = id,
                Title = input.Title,
                Slug = input.Slug,
                Content = safeContent,
                UpdatedBy = userId
            };

            var updated = await _cmsRepository.UpdateAsync(page);

            if (!updated)
            {
                return NotFound(new { message = "Página no encontrada." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error al actualizar la página del CMS: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("id")?.Value;
        long? userId = long.TryParse(userIdStr, out var parsedId) ? parsedId : null;

        var deleted = await _cmsRepository.DeleteAsync(id, userId);
        return deleted ? NoContent() : NotFound();
    }
}

public class CmsPageInputDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
}
