using System.Net.Http.Json;
namespace StoreClient;

public class ItemApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    public async Task<List<Item>> GetAllAsync(string? search = null, int? categoryId = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");
        if (categoryId.HasValue)
            query.Add($"categoryId={categoryId}");
        var url = "api/items" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await _http.GetFromJsonAsync<List<Item>>(url) ?? new();
    }
    public async Task<string?> AddAsync(Item item)
    {
        var res = await _http.PostAsJsonAsync("api/items", item);
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> UpdateAsync(Item item)
    {
        var res = await _http.PutAsJsonAsync($"api/items/{Uri.EscapeDataString(item.Code)}", item);
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> DeleteAsync(string code)
    {
        var res = await _http.DeleteAsync($"api/items/{Uri.EscapeDataString(code)}");
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
}