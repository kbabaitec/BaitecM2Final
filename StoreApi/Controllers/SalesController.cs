using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Data;
using StoreApi.Models;

namespace StoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _db;
    public SalesController(AppDbContext db) => _db = db;

    
    [HttpPost]
    public async Task<ActionResult<SaleDetailDto>> Checkout(SaleRequest req)
    {
        if (req.Lines.Count == 0) return BadRequest("The cart is empty.");
        if (req.Lines.Any(l => l.Quantity <= 0))
            return BadRequest("Every quantity must be more than 0.");

        
        var wanted = req.Lines
            .GroupBy(l => l.ItemCode)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        var codes = wanted.Keys.ToList();
        var items = await _db.Items.Where(i => codes.Contains(i.Code)).ToListAsync();

        
        foreach (var code in codes)
        {
            var item = items.FirstOrDefault(i => i.Code == code);
            if (item is null) return NotFound($"Item '{code}' not found.");
            if (item.Quantity < wanted[code])
                return BadRequest($"Not enough stock for {item.Name}. Only {item.Quantity} left.");
        }

        
        var sale = new Sale { Date = DateTime.Now, UserId = req.UserId };
        foreach (var item in items)
        {
            sale.Items.Add(new SaleItem
            {
                ItemId = item.Id,
                Quantity = wanted[item.Code],
                UnitPrice = item.UnitPrice
            });
        }
        sale.Total = sale.Items.Sum(s => s.Quantity * s.UnitPrice);
        sale.AmountPaid = req.AmountPaid;

        if (req.AmountPaid < sale.Total)
            return BadRequest($"Amount paid is less than the total (₱{sale.Total:N2}).");

        
        await using var tx = await _db.Database.BeginTransactionAsync();

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();   

        foreach (var item in items)
        {
            var qty = wanted[item.Code];
            item.Quantity -= qty;
            _db.StockMovements.Add(new StockMovement
            {
                ItemId = item.Id,
                Type = "Sale",
                QuantityChange = -qty,
                Note = $"Sale #{sale.Id}",
                UserId = req.UserId,
                Date = sale.Date
            });
        }
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(await BuildDetail(sale.Id));
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDetailDto>> GetById(int id)
    {
        var detail = await BuildDetail(id);
        return detail is null ? NotFound("Sale not found.") : Ok(detail);
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleSummaryDto>>> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.Sales.Include(s => s.Items).AsQueryable();
        if (from.HasValue) query = query.Where(s => s.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(s => s.Date < to.Value.Date.AddDays(1));

        var sales = await query.OrderByDescending(s => s.Date).ToListAsync();
        var users = await _db.Users.ToDictionaryAsync(u => u.Id, u => u.FullName);

        var result = sales.Select(s => new SaleSummaryDto(
            s.Id, s.Date, s.Items.Sum(i => i.Quantity), s.Total, s.AmountPaid,
            s.UserId.HasValue && users.TryGetValue(s.UserId.Value, out var name) ? name : ""));

        return Ok(result);
    }

    private async Task<SaleDetailDto?> BuildDetail(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (sale is null) return null;

        string? cashier = null;
        if (sale.UserId.HasValue)
            cashier = await _db.Users.Where(u => u.Id == sale.UserId)
                                     .Select(u => u.FullName).FirstOrDefaultAsync();

        var lines = sale.Items.Select(i => new SaleLineDto(
            i.Item!.Code, i.Item.Name, i.Quantity, i.UnitPrice, i.Quantity * i.UnitPrice)).ToList();

        return new SaleDetailDto(sale.Id, sale.Date, sale.Total, sale.AmountPaid,
                                 sale.AmountPaid - sale.Total, cashier ?? "", lines);
    }
}