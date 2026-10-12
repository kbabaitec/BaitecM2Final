using System.ComponentModel.DataAnnotations;
namespace StoreApi.Models;

public class Supplier

{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    public string? ContactNumber { get; set; }
    public string? Address { get; set; }
}
