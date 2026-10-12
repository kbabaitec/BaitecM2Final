namespace StoreApi.Models;

public class StockMovement
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public string Type { get; set; } = "";      // Stock In, Stock Out, Adjustment, Sale
    public int QuantityChange { get; set; }     // positive adds stock, negative removes stock
    public string? Note { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public int? UserId { get; set; }
    public int? SupplierId { get; set; }
}
