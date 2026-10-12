using StoreApi.Models;

namespace StoreApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new AppUser
                {
                    Username = "admin",
                    FullName = "Store Admin",
                    Role = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
                },
                new AppUser
                {
                    Username = "cashier",
                    FullName = "Store Cashier",
                    Role = "Cashier",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier123")
                });
        }

        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Name = "Noodles and Canned Goods" },
                new Category { Name = "Beverages" },
                new Category { Name = "Household" },
                new Category { Name = "Snacks" });
            db.SaveChanges();
        }

        if (!db.Items.Any())
        {
            int Cat(string name) => db.Categories.First(c => c.Name == name).Id;

            db.Items.AddRange(
                new Item
                {
                    Name = "Instant Noodles Chicken",
                    Code = "NDL-001",
                    Brand = "Lucky Me",
                    UnitPrice = 14.50m,
                    Quantity = 50,
                    ReorderLevel = 10,
                    CategoryId = Cat("Noodles and Canned Goods")
                },
                new Item
                {
                    Name = "Sardines 155g",
                    Code = "CAN-001",
                    Brand = "Ligo",
                    UnitPrice = 28.50m,
                    Quantity = 30,
                    ReorderLevel = 10,
                    CategoryId = Cat("Noodles and Canned Goods")
                },
                new Item
                {
                    Name = "3-in-1 Coffee Original",
                    Code = "COF-001",
                    Brand = "Nescafe",
                    UnitPrice = 8.00m,
                    Quantity = 100,
                    ReorderLevel = 20,
                    CategoryId = Cat("Beverages")
                },
                new Item
                {
                    Name = "Softdrink 1L",
                    Code = "DRK-001",
                    Brand = "Coca-Cola",
                    UnitPrice = 50.00m,
                    Quantity = 24,
                    ReorderLevel = 6,
                    CategoryId = Cat("Beverages")
                },
                new Item
                {
                    Name = "Laundry Bar 350g",
                    Code = "SOP-001",
                    Brand = "Tide",
                    UnitPrice = 22.00m,
                    Quantity = 8,
                    ReorderLevel = 10,
                    CategoryId = Cat("Household")
                },
                new Item
                {
                    Name = "Potato Chips 60g",
                    Code = "SNK-001",
                    Brand = "Piattos",
                    UnitPrice = 25.00m,
                    Quantity = 40,
                    ReorderLevel = 10,
                    CategoryId = Cat("Snacks")
                });
        }

        db.SaveChanges();
    }
}
