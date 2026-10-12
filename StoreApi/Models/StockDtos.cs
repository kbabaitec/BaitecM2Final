namespace StoreApi.Models;

public record StockInRequest(string ItemCode, int Quantity, int? SupplierId,
                             string? Note, int? UserId);
public record StockOutRequest(string ItemCode, int Quantity, string Reason,
                              string? Note, int? UserId);
public record StockAdjustRequest(string ItemCode, int NewQuantity, string Note, int? UserId);
public record MovementDto(int Id, DateTime Date, string ItemCode, string ItemName,
                          string Type, int QuantityChange, string? SupplierName,
                          string? Note, string? UserName);