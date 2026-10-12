namespace StoreClient;

public class CartLine
{
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}