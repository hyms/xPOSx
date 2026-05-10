using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    public CategoriesController(ICategoryRepository categoryRepository) { _categoryRepository = categoryRepository; }

    [Authorize(Policy = "categories_view")]
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _categoryRepository.GetAllAsync());
    
    [Authorize(Policy = "categories_view")]
    [HttpGet("{id}")] public async Task<IActionResult> GetById(long id) { var category = await _categoryRepository.GetByIdAsync(id); return category == null ? NotFound() : Ok(category); }
    
    [Authorize(Policy = "categories_create")]
    [HttpPost] public async Task<IActionResult> Create(Category category) { var id = await _categoryRepository.CreateAsync(category); category.Id = id; return CreatedAtAction(nameof(GetById), new { id }, category); }
    
    [Authorize(Policy = "categories_edit")]
    [HttpPut("{id}")] public async Task<IActionResult> Update(long id, Category category) { if (id != category.Id) return BadRequest(); return await _categoryRepository.UpdateAsync(category) ? NoContent() : NotFound(); }
    
    [Authorize(Policy = "categories_delete")]
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(long id) => await _categoryRepository.DeleteAsync(id) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;
    public UnitsController(IUnitRepository unitRepository) { _unitRepository = unitRepository; }

    [Authorize(Policy = "units_view")]
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _unitRepository.GetAllAsync());
    
    [Authorize(Policy = "units_view")]
    [HttpGet("{id}")] public async Task<IActionResult> GetById(long id) { var unit = await _unitRepository.GetByIdAsync(id); return unit == null ? NotFound() : Ok(unit); }
    
    [Authorize(Policy = "units_create")]
    [HttpPost] public async Task<IActionResult> Create(Unit unit) { var id = await _unitRepository.CreateAsync(unit); unit.Id = id; return CreatedAtAction(nameof(GetById), new { id }, unit); }
    
    [Authorize(Policy = "units_edit")]
    [HttpPut("{id}")] public async Task<IActionResult> Update(long id, Unit unit) { if (id != unit.Id) return BadRequest(); return await _unitRepository.UpdateAsync(unit) ? NoContent() : NotFound(); }
    
    [Authorize(Policy = "units_delete")]
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(long id) => await _unitRepository.DeleteAsync(id) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    public ProductsController(IProductRepository productRepository) { _productRepository = productRepository; }

    [Authorize(Policy = "products_view")]
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _productRepository.GetAllAsync());
    
    [Authorize(Policy = "products_view")]
    [HttpGet("{id}")] public async Task<IActionResult> GetById(long id) { var product = await _productRepository.GetByIdAsync(id); return product == null ? NotFound() : Ok(product); }
    
    [Authorize(Policy = "products_create")]
    [HttpPost] public async Task<IActionResult> Create(Product product) { var id = await _productRepository.CreateAsync(product); product.Id = id; return CreatedAtAction(nameof(GetById), new { id }, product); }
    
    [Authorize(Policy = "products_edit")]
    [HttpPut("{id}")] public async Task<IActionResult> Update(long id, Product product) { if (id != product.Id) return BadRequest(); return await _productRepository.UpdateAsync(product) ? NoContent() : NotFound(); }
    
    [Authorize(Policy = "products_delete")]
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(long id) => await _productRepository.DeleteAsync(id) ? NoContent() : NotFound();
}
