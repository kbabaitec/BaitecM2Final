using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;
namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly AppDbContext _db;
    public SuppliersController(AppDbContext db) => _db = db;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Supplier>>> GetAll() =>
        Ok(await _db.Suppliers.OrderBy(s => s.Name).ToListAsync());
    
    [HttpPost]
    public async Task<ActionResult<Supplier>> Create(Supplier supplier)
    {
        var name = supplier.Name.Trim();
        if (name.Length == 0) return BadRequest("Supplier name is required.");
        if (await _db.Suppliers.AnyAsync(s => s.Name == name))
            return Conflict($"Supplier '{name}' already exists.");
        var entity = new Supplier
        {
            Name = name,
            ContactNumber = supplier.ContactNumber?.Trim(),
            Address = supplier.Address?.Trim()
        };
        _db.Suppliers.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(entity);
    }
    
    [HttpPut("{id:int}")]

    public async Task<IActionResult> Update(int id, Supplier supplier)
    {
        var entity = await _db.Suppliers.FindAsync(id);
        if (entity is null) return NotFound("Supplier not found.");
        var name = supplier.Name.Trim();
        if (name.Length == 0) return BadRequest("Supplier name is required.");
        if (await _db.Suppliers.AnyAsync(s => s.Name == name && s.Id != id))
            return Conflict($"Supplier '{name}' already exists.");
        entity.Name = name;
        entity.ContactNumber = supplier.ContactNumber?.Trim();
        entity.Address = supplier.Address?.Trim();
        await _db.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Suppliers.FindAsync(id);
        if (entity is null) return NotFound("Supplier not found.");
        _db.Suppliers.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
