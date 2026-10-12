using System.Net.Http.Json;
namespace StoreClient;

public class SupplierApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    public async Task<List<Supplier>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<Supplier>>("api/suppliers") ?? new();
    public async Task<string?> AddAsync(Supplier supplier)
    {
        var res = await _http.PostAsJsonAsync("api/suppliers", supplier);
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> UpdateAsync(Supplier supplier)
    {
        var res = await _http.PutAsJsonAsync($"api/suppliers/{supplier.Id}", supplier);
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> DeleteAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/suppliers/{id}");
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
}