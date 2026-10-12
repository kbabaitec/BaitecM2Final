using System.Net.Http.Json;
namespace StoreClient;

public class SalesApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    public async Task<(SaleDetailInfo? Sale, string? Error)> CheckoutAsync(
        IEnumerable<CartLine> lines, decimal amountPaid)
    {
        var body = new
        {
            lines = lines.Select(l => new { itemCode = l.ItemCode, quantity = l.Quantity }).ToList(),
            amountPaid,
            userId = Session.CurrentUser?.Id
        };
        var res = await _http.PostAsJsonAsync("api/sales", body);
        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<SaleDetailInfo>(), null);
        return (null, await res.Content.ReadAsStringAsync());
    }
    public async Task<List<SaleSummaryInfo>> GetSalesAsync(DateTime? from, DateTime? to)
    {
        var query = new List<string>();
        if (from.HasValue) query.Add($"from={from.Value:yyyy-MM-dd}");
        if (to.HasValue) query.Add($"to={to.Value:yyyy-MM-dd}");
        var url = "api/sales" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<SaleSummaryInfo>>(url) ?? new();
    }
    public async Task<SaleDetailInfo?> GetSaleAsync(int id) =>
        await _http.GetFromJsonAsync<SaleDetailInfo>($"api/sales/{id}");
}