namespace StoreApi.Models;

public record DailySalesDto(DateTime Date, int SalesCount, int ItemsSold, decimal Total);
public record SalesByItemDto(string Code, string Name, int QuantitySold, decimal Revenue);
public record InventoryRowDto(string Code, string Name, string Category,
                              int Quantity, decimal UnitPrice, decimal Value);