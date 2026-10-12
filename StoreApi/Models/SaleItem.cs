namespace StoreApi.Models;

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }   // price at the time of the sale
}
