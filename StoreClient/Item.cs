namespace StoreClient;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Brand { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public string CategoryName => Category?.Name ?? "";
}
