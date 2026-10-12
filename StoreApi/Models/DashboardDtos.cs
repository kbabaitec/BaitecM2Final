namespace StoreApi.Models;

public record LowStockDto(string Code, string Name, int Quantity, int ReorderLevel, int Suggested);
public record TopSellerDto(string Code, string Name, int QuantitySold);
public record DayTotalDto(DateTime Date, decimal Total);
public record DashboardDto(
 int TotalItems, int TotalUnits, decimal InventoryValue,
 int LowStockCount, int OutOfStockCount,
 decimal TodaySales, int TodaySaleCount, decimal MonthSales,
 List<LowStockDto> LowStock, List<TopSellerDto> TopSellers, List<DayTotalDto> Last7Days);