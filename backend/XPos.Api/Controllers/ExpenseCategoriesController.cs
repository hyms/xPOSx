using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/expense-categories")]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    public ExpenseCategoriesController(IExpenseCategoryRepository categoryRepository) { _categoryRepository = categoryRepository; }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _categoryRepository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ExpenseCategory category)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            category.UserId = userId;
            category.CreatedBy = userId;
        }

        var id = await _categoryRepository.CreateAsync(category);
        category.Id = id;
        return CreatedAtAction(nameof(GetById), new { id }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, ExpenseCategory category)
    {
        if (id != category.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            category.UpdatedBy = userId;
        }

        return await _categoryRepository.UpdateAsync(category) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _categoryRepository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
