using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
namespace StoreApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Item>().HasIndex(i => i.Code).IsUnique();
        b.Entity<Item>().Property(i => i.UnitPrice).HasPrecision(18, 2);
        b.Entity<Item>().HasOne(i => i.Category).WithMany()
            .HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
        b.Entity<Sale>().Property(s => s.Total).HasPrecision(18, 2);
        b.Entity<Sale>().Property(s => s.AmountPaid).HasPrecision(18, 2);
        b.Entity<SaleItem>().Property(s => s.UnitPrice).HasPrecision(18, 2);
        
        b.Entity<StockMovement>().HasOne(m => m.Item).WithMany()
            .HasForeignKey(m => m.ItemId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<SaleItem>().HasOne(s => s.Item).WithMany()
            .HasForeignKey(s => s.ItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

