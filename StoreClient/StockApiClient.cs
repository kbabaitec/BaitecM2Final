using System.Net.Http.Json;
namespace StoreClient;

public class StockApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    public async Task<string?> StockInAsync(string itemCode, int quantity,
                                            int? supplierId, string? note)
    {
        var res = await _http.PostAsJsonAsync("api/stock/in", new
        {
            itemCode,
            quantity,
            supplierId,
            note,
            userId = Session.CurrentUser?.Id
        });
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> StockOutAsync(string itemCode, int quantity,
                                             string reason, string? note)
    {
        var res = await _http.PostAsJsonAsync("api/stock/out", new
        {
            itemCode,
            quantity,
            reason,
            note,
            userId = Session.CurrentUser?.Id
        });
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }

    public async Task<string?> AdjustAsync(string itemCode, int newQuantity, string note)
    {
        var res = await _http.PostAsJsonAsync("api/stock/adjust", new
        {
            itemCode,
            newQuantity,
            note,
            userId = Session.CurrentUser?.Id
        });
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<List<StockMovementInfo>> GetMovementsAsync(
        string? itemCode, string? type, DateTime? from, DateTime? to)
    {
        var query = new List<string>();
        if (!string.IsNullOrEmpty(itemCode))
            query.Add($"itemCode={Uri.EscapeDataString(itemCode)}");
        if (!string.IsNullOrEmpty(type))
            query.Add($"type={Uri.EscapeDataString(type)}");
        if (from.HasValue) query.Add($"from={from.Value:yyyy-MM-dd}");
        if (to.HasValue) query.Add($"to={to.Value:yyyy-MM-dd}");

        var url = "api/stock/movements" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<StockMovementInfo>>(url) ?? new();
    }
}
