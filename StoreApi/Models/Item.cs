using System.ComponentModel.DataAnnotations;
namespace StoreApi.Models;

public class Item
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(30)] public string Code { get; set; } = "";
    [Required, MaxLength(60)] public string Brand { get; set; } = "";
    [Range(0.01, 1000000)] public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}
