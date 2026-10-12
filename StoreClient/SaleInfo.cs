namespace StoreClient;

public class SaleLineInfo
{
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
public class SaleDetailInfo
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }
    public string CashierName { get; set; } = "";
    public List<SaleLineInfo> Lines { get; set; } = new();
}
public class SaleSummaryInfo
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int ItemCount { get; set; }
    public decimal Total { get; set; }
    public decimal AmountPaid { get; set; }
    public string CashierName { get; set; } = "";
}