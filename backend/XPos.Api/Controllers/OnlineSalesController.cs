using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[ApiController]
[Route("api")]
public class OnlineSalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ICmsRepository _cmsRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public OnlineSalesController(
        ISaleService saleService,
        ICmsRepository cmsRepository,
        IWebHostEnvironment environment,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _saleService = saleService;
        _cmsRepository = cmsRepository;
        _environment = environment;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    [AllowAnonymous]
    [HttpGet("cms/pages/{slug}")]
    public async Task<IActionResult> GetCmsPage(string slug)
    {
        var page = await _cmsRepository.GetBySlugAsync(slug);
        if (page == null)
        {
            return NotFound(new { message = "Página no encontrada" });
        }
        return Ok(page);
    }

    [AllowAnonymous]
    [HttpGet("products/public")]
    public async Task<IActionResult> GetPublicProducts()
    {
        var products = await _productRepository.GetPublicProductsAsync();
        return Ok(products);
    }

    [AllowAnonymous]
    [HttpGet("products/public/top")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int limit = 5)
    {
        var products = await _productRepository.GetTopSellingAsync(limit);
        return Ok(products);
    }

    [AllowAnonymous]
    [HttpGet("categories/public")]
    public async Task<IActionResult> GetPublicCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories);
    }

    [AllowAnonymous]
    [EnableRateLimiting("ReceiptUploadPolicy")]
    [HttpPost("sales/online/upload-receipt")]
    public async Task<IActionResult> UploadReceipt([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No se ha subido ningún archivo." });
        }

        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Extensión de archivo inválida. Solo se permiten imágenes (PNG, JPG, JPEG)." });
        }

        // Validar físicamente que el peso del archivo no exceda los 2MB
        if (file.Length > 2 * 1024 * 1024)
        {
            return BadRequest(new { message = "El tamaño del archivo supera el límite de 2MB." });
        }

        // Validar la firma real del archivo (Magic Bytes) en caliente
        using (var stream = file.OpenReadStream())
        {
            var headerBytes = new byte[4];
            var read = await stream.ReadAsync(headerBytes, 0, 4);
            if (read < 3)
            {
                return BadRequest(new { message = "Firma de archivo inválida. El archivo es demasiado corto o corrupto." });
            }

            bool isValidSignature = false;

            // PNG signature: 89 50 4E 47
            if (headerBytes[0] == 0x89 && headerBytes[1] == 0x50 && headerBytes[2] == 0x4E && headerBytes[3] == 0x47)
            {
                isValidSignature = true;
            }
            // JPEG signature: FF D8 FF
            else if (headerBytes[0] == 0xFF && headerBytes[1] == 0xD8 && headerBytes[2] == 0xFF)
            {
                isValidSignature = true;
            }

            if (!isValidSignature)
            {
                return BadRequest(new { message = "Firma de archivo inválida. El archivo real no corresponde estrictamente a imágenes JPEG o PNG." });
            }
        }

        // Sanitizar el nombre original del archivo eliminando caracteres especiales
        var originalFileName = Path.GetFileName(file.FileName);
        var sanitizedOriginal = string.Concat(originalFileName.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_'));

        // Almacenar fuera del directorio raíz de ejecución (Storage/receipts)
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "receipts");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);
        var relativePath = $"/api/sales/receipts/{fileName}";

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { path = relativePath });
    }

    [AllowAnonymous]
    [HttpGet("sales/receipts/{fileName}")]
    public IActionResult GetReceipt(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || Path.GetFileName(fileName) != fileName)
        {
            return BadRequest(new { message = "Nombre de archivo inválido." });
        }

        var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "receipts");
        var filePath = Path.Combine(storagePath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = "Comprobante no encontrado." });
        }

        var extension = Path.GetExtension(fileName).ToLower();
        var contentType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType);
    }

    [AllowAnonymous]
    [EnableRateLimiting("OrderCreationPolicy")]
    [HttpPost("sales/online")]
    public async Task<IActionResult> CreateOnlineSale([FromBody] Sale sale)
    {
        if (sale == null)
        {
            return BadRequest(new { message = "Datos de venta inválidos." });
        }

        try
        {
            var saleId = await _saleService.CreateOnlineSaleAsync(sale);
            return CreatedAtAction(nameof(GetById), new { id = saleId }, sale);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error interno al registrar el pedido: {ex.Message}" });
        }
    }

    [AllowAnonymous]
    [HttpGet("sales/online/{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var sale = await _saleService.GetByIdAsync(id);
        return sale == null ? NotFound() : Ok(sale);
    }

    [Authorize]
    [HttpPut("sales/online/{id}/approve")]
    public async Task<IActionResult> ApproveOnlineSale(long id)
    {
        try
        {
            var success = await _saleService.ApproveOnlineSaleAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Pedido no encontrado" });
            }
            return Ok(new { message = "Pedido aprobado exitosamente y stock descontado." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error al aprobar el pedido: {ex.Message}" });
        }
    }

    [Authorize]
    [HttpPut("sales/online/{id}/reject")]
    public async Task<IActionResult> RejectOnlineSale(long id)
    {
        try
        {
            var success = await _saleService.RejectOnlineSaleAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Pedido no encontrado" });
            }
            return Ok(new { message = "Pedido rechazado/cancelado exitosamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error al rechazar el pedido: {ex.Message}" });
        }
    }
}
