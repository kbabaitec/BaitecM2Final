namespace StoreClient;

public class LowStockInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; }
    public int Suggested { get; set; }
}
public class TopSellerInfo
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int QuantitySold { get; set; }
}
public class DayTotalInfo
{
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
}
public class DashboardInfo
{
    public int TotalItems { get; set; }
    public int TotalUnits { get; set; }
    public decimal InventoryValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal TodaySales { get; set; }
    public int TodaySaleCount { get; set; }
    public decimal MonthSales { get; set; }
    public List<LowStockInfo> LowStock { get; set; } = new();
    public List<TopSellerInfo> TopSellers { get; set; } = new();
    public List<DayTotalInfo> Last7Days { get; set; } = new();
}
