using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;
namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;
    
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var items = await _db.Items.ToListAsync();
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var weekStart = today.AddDays(-6);
        var thirtyDaysAgo = today.AddDays(-30);
        var since = monthStart < thirtyDaysAgo ? monthStart : thirtyDaysAgo;
        var sales = await _db.Sales
        .Include(s => s.Items).ThenInclude(i => i.Item)
        .Where(s => s.Date >= since)
        .ToListAsync();
        
        
        var lowStock = items
        .Where(i => i.Quantity <= i.ReorderLevel)
        .OrderBy(i => i.Quantity - i.ReorderLevel)
        .Select(i => new LowStockDto(i.Code, i.Name, i.Quantity, i.ReorderLevel,
        Math.Max(i.ReorderLevel * 2 - i.Quantity, 1)))
        .ToList();
        var topSellers = sales
        .Where(s => s.Date >= thirtyDaysAgo)
        .SelectMany(s => s.Items)
        .GroupBy(i => i.ItemId)
        .Select(g => new TopSellerDto(g.First().Item!.Code, g.First().Item!.Name,
        g.Sum(x => x.Quantity)))
        .OrderByDescending(t => t.QuantitySold)
        .Take(5)
        .ToList();
        var last7Days = Enumerable.Range(0, 7)
        .Select(n => weekStart.AddDays(n))
        .Select(day => new DayTotalDto(day,
        sales.Where(s => s.Date.Date == day).Sum(s => s.Total)))
        .ToList();
        var todaySales = sales.Where(s => s.Date.Date == today).ToList();
        return Ok(new DashboardDto(
        items.Count,
        items.Sum(i => i.Quantity),
        items.Sum(i => i.Quantity * i.UnitPrice),
        lowStock.Count,
        items.Count(i => i.Quantity == 0),
        todaySales.Sum(s => s.Total),
        todaySales.Count,
        sales.Where(s => s.Date >= monthStart).Sum(s => s.Total),
        lowStock, topSellers, last7Days));
    }
}