namespace StoreApi.Models;

public record SaleLineRequest(string ItemCode, int Quantity);
public record SaleRequest(List<SaleLineRequest> Lines, decimal AmountPaid, int? UserId);
public record SaleLineDto(string ItemCode, string ItemName, int Quantity,
                          decimal UnitPrice, decimal LineTotal);
public record SaleDetailDto(int Id, DateTime Date, decimal Total, decimal AmountPaid,
                            decimal Change, string CashierName, List<SaleLineDto> Lines);
public record SaleSummaryDto(int Id, DateTime Date, int ItemCount, decimal Total,
                             decimal AmountPaid, string CashierName);