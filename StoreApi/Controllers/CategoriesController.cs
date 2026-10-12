using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;

namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll() =>
        Ok(await _db.Categories.OrderBy(c => c.Name).ToListAsync());
    
    [HttpPost]
    public async Task<ActionResult<Category>> Create(Category category)
    {
        var name = category.Name.Trim();
        if (name.Length == 0) return BadRequest("Category name is required.");
        if (await _db.Categories.AnyAsync(c => c.Name == name))
            return Conflict($"Category '{name}' already exists.");
        var entity = new Category { Name = name };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(entity);
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Category category)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity is null) return NotFound("Category not found.");
        var name = category.Name.Trim();

        if (name.Length == 0) return BadRequest("Category name is required.");
        if (await _db.Categories.AnyAsync(c => c.Name == name && c.Id != id))
            return Conflict($"Category '{name}' already exists.");
        entity.Name = name;
        await _db.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity is null) return NotFound("Category not found.");
        var used = await _db.Items.CountAsync(i => i.CategoryId == id);
        if (used > 0)
            return Conflict($"This category is used by {used} item(s). Change those items first.");
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
