using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;
namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly AppDbContext _db;
    public StockController(AppDbContext db) => _db = db;
    
    private void AddMovement(Item item, string type, int change,
                             int? supplierId, string? note, int? userId) =>
        _db.StockMovements.Add(new StockMovement
        {
            ItemId = item.Id,
            Type = type,
            QuantityChange = change,
            SupplierId = supplierId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            UserId = userId,
            Date = DateTime.Now
        });
    
    [HttpPost("in")]
    public async Task<IActionResult> StockIn(StockInRequest req)
    {
        if (req.Quantity <= 0) return BadRequest("Quantity must be more than 0.");
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Code == req.ItemCode);
        if (item is null) return NotFound($"Item '{req.ItemCode}' not found.");
        if (req.SupplierId.HasValue && !await _db.Suppliers.AnyAsync(s => s.Id == req.SupplierId))
            return BadRequest("Supplier not found.");
        item.Quantity += req.Quantity;
        AddMovement(item, "Stock In", req.Quantity, req.SupplierId, req.Note, req.UserId);
        await _db.SaveChangesAsync();
        return Ok(new { item.Code, item.Quantity });
    }
    
    [HttpPost("out")]
    public async Task<IActionResult> StockOut(StockOutRequest req)
    {
        if (req.Quantity <= 0) return BadRequest("Quantity must be more than 0.");
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Code == req.ItemCode);
        if (item is null) return NotFound($"Item '{req.ItemCode}' not found.");
        if (item.Quantity < req.Quantity)
            return BadRequest($"Not enough stock. Only {item.Quantity} left.");
        var note = req.Reason + (string.IsNullOrWhiteSpace(req.Note) ? "" : " - " + req.Note.Trim());
        item.Quantity -= req.Quantity;
        AddMovement(item, "Stock Out", -req.Quantity, null, note, req.UserId);
        await _db.SaveChangesAsync();
        return Ok(new { item.Code, item.Quantity });
    }
    
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(StockAdjustRequest req)
    {
        if (req.NewQuantity < 0) return BadRequest("Quantity cannot be negative.");
        if (string.IsNullOrWhiteSpace(req.Note)) return BadRequest("Please give a reason.");
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Code == req.ItemCode);
        if (item is null) return NotFound($"Item '{req.ItemCode}' not found.");
        var difference = req.NewQuantity - item.Quantity;
        if (difference == 0) return BadRequest($"The quantity is already {item.Quantity}.");
        item.Quantity = req.NewQuantity;
        AddMovement(item, "Adjustment", difference, null, req.Note, req.UserId);
        await _db.SaveChangesAsync();
        return Ok(new { item.Code, item.Quantity });
    }
    
    [HttpGet("movements")]
    public async Task<ActionResult<IEnumerable<MovementDto>>> Movements(
        [FromQuery] string? itemCode, [FromQuery] string? type,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.StockMovements.Include(m => m.Item).AsQueryable();
        if (!string.IsNullOrWhiteSpace(itemCode))
            query = query.Where(m => m.Item!.Code == itemCode);
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(m => m.Type == type);
        if (from.HasValue)
            query = query.Where(m => m.Date >= from.Value.Date);
        if (to.HasValue)
            query = query.Where(m => m.Date < to.Value.Date.AddDays(1));
        var movements = await query
            .OrderByDescending(m => m.Date).ThenByDescending(m => m.Id)
            .ToListAsync();
        var suppliers = await _db.Suppliers.ToDictionaryAsync(s => s.Id, s => s.Name);
        var users = await _db.Users.ToDictionaryAsync(u => u.Id, u => u.FullName);
        var result = movements.Select(m => new MovementDto(
            m.Id, m.Date, m.Item!.Code, m.Item.Name, m.Type, m.QuantityChange,
            m.SupplierId.HasValue && suppliers.TryGetValue(m.SupplierId.Value, out var sn) ? sn : null,
            m.Note,
            m.UserId.HasValue && users.TryGetValue(m.UserId.Value, out var un) ? un : null));
        return Ok(result);
    }
}