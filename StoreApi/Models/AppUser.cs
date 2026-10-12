using System.ComponentModel.DataAnnotations;
namespace StoreApi.Models;

public class AppUser
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    [MaxLength(100)] public string FullName { get; set; } = "";
    [MaxLength(20)] public string Role { get; set; } = "Cashier";   // Admin or Cashier
}
