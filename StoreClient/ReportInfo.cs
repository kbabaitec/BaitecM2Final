namespace StoreClient;

public class DailySalesInfo
{
    public DateTime Date { get; set; }
    public int SalesCount { get; set; }
    public int ItemsSold { get; set; }
    public decimal Total { get; set; }
}
public class SalesByItemInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
public class InventoryRowInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Value { get; set; }
}