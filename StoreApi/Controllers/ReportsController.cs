using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;
namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;
    
    [HttpGet("daily-sales")]
    public async Task<ActionResult<IEnumerable<DailySalesDto>>> DailySales(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var start = (from ?? DateTime.Today.AddDays(-29)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (end < start) return BadRequest("The 'To' date is before the 'From' date.");
        var sales = await _db.Sales
            .Include(s => s.Items)
            .Where(s => s.Date >= start && s.Date < end.AddDays(1))
            .ToListAsync();
        var rows = sales
            .GroupBy(s => s.Date.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailySalesDto(
                g.Key,
                g.Count(),
                g.Sum(s => s.Items.Sum(i => i.Quantity)),
                g.Sum(s => s.Total)))
            .ToList();
        return Ok(rows);
    }
    
    [HttpGet("sales-by-item")]
    public async Task<ActionResult<IEnumerable<SalesByItemDto>>> SalesByItem(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var start = (from ?? DateTime.Today.AddDays(-29)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (end < start) return BadRequest("The 'To' date is before the 'From' date.");
        var sales = await _db.Sales
            .Include(s => s.Items).ThenInclude(i => i.Item)
            .Where(s => s.Date >= start && s.Date < end.AddDays(1))
            .ToListAsync();
        var rows = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => i.ItemId)
            .Select(g => new SalesByItemDto(
                g.First().Item!.Code,
                g.First().Item!.Name,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.UnitPrice)))
            .OrderByDescending(r => r.Revenue)
            .ToList();
        return Ok(rows);
    }
    
    [HttpGet("inventory")]
    public async Task<ActionResult<IEnumerable<InventoryRowDto>>> Inventory()
    {
        var rows = await _db.Items
            .OrderBy(i => i.Category!.Name).ThenBy(i => i.Name)
            .Select(i => new InventoryRowDto(
                i.Code, i.Name,
                i.Category == null ? "" : i.Category.Name,
                i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice))
            .ToListAsync();
        return Ok(rows);
    }
}

