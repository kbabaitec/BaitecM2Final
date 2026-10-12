using System.Net.Http.Json;
namespace StoreClient;

public class ReportsApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    private static string Range(DateTime? from, DateTime? to)
    {
        var query = new List<string>();
        if (from.HasValue) query.Add($"from={from.Value:yyyy-MM-dd}");
        if (to.HasValue) query.Add($"to={to.Value:yyyy-MM-dd}");
        return query.Count > 0 ? "?" + string.Join("&", query) : "";
    }
    public async Task<List<DailySalesInfo>> GetDailySalesAsync(DateTime? from, DateTime? to) =>
        await _http.GetFromJsonAsync<List<DailySalesInfo>>(
            "api/reports/daily-sales" + Range(from, to)) ?? new();
    public async Task<List<SalesByItemInfo>> GetSalesByItemAsync(DateTime? from, DateTime? to) =>
        await _http.GetFromJsonAsync<List<SalesByItemInfo>>(
            "api/reports/sales-by-item" + Range(from, to)) ?? new();
    public async Task<List<InventoryRowInfo>> GetInventoryAsync() =>
        await _http.GetFromJsonAsync<List<InventoryRowInfo>>("api/reports/inventory") ?? new();
}