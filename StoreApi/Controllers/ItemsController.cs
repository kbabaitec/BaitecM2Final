using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;

namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ItemsController(AppDbContext db) => _db = db;

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetAll(
        [FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var query = _db.Items.Include(i => i.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.Name.Contains(search)
                                  || i.Brand.Contains(search)
                                  || i.Code.Contains(search));
        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId);

        return Ok(await query.OrderBy(i => i.Code).ToListAsync());
    }

    
    [HttpGet("{code}")]
    public async Task<ActionResult<Item>> GetByCode(string code)
    {
        var item = await _db.Items
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Code == code);

        return item is null ? NotFound($"Item '{code}' not found.") : Ok(item);
    }

    
    [HttpPost]
    public async Task<ActionResult<Item>> Create(Item item)
    {
        if (await _db.Items.AnyAsync(i => i.Code == item.Code))
            return Conflict($"Item code '{item.Code}' already exists.");

        item.Id = 0;
        item.Category = null;
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByCode), new { code = item.Code }, item);
    }

    
    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, Item item)
    {
        var existing = await _db.Items.FirstOrDefaultAsync(i => i.Code == code);
        if (existing is null) return NotFound($"Item '{code}' not found.");

        existing.Name = item.Name;
        existing.Brand = item.Brand;
        existing.UnitPrice = item.UnitPrice;
        existing.ReorderLevel = item.ReorderLevel;
        existing.CategoryId = item.CategoryId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code)
    {
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Code == code);
        if (item is null) return NotFound($"Item '{code}' not found.");

        try
        {
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Conflict("This item has stock or sales records and cannot be deleted.");
        }
    }
}
