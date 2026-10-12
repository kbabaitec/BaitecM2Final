namespace StoreApi.Models;

public class Sale
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public int? UserId { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}
