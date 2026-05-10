using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XPos.Domain.Interfaces;
using XPos.Domain.Models;

namespace XPos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseRepository _expenseRepository;
    public ExpensesController(IExpenseRepository expenseRepository) { _expenseRepository = expenseRepository; }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _expenseRepository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id);
        return expense == null ? NotFound() : Ok(expense);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Expense expense)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            expense.UserId = userId;
            expense.CreatedBy = userId;
        }

        if (string.IsNullOrEmpty(expense.Ref))
        {
            expense.Ref = $"EXP-{DateTime.Now:yyyyMMddHHmmss}";
        }

        var id = await _expenseRepository.CreateAsync(expense);
        return CreatedAtAction(nameof(GetById), new { id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(userIdStr, out long userId))
        {
            expense.UpdatedBy = userId;
        }

        return await _expenseRepository.UpdateAsync(expense) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdStr, out long userId);
        return await _expenseRepository.DeleteAsync(id, userId) ? NoContent() : NotFound();
    }
}
