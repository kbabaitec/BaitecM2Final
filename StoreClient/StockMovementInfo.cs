namespace StoreClient;

public class StockMovementInfo
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string Type { get; set; } = "";
    public int QuantityChange { get; set; }
    public string? SupplierName { get; set; }
    public string? Note { get; set; }
    public string? UserName { get; set; }
}