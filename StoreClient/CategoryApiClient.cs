using System.Net.Http.Json;
namespace StoreClient;

public class CategoryApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    public async Task<List<Category>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<Category>>("api/categories") ?? new();
    public async Task<string?> AddAsync(Category category)
    {
        var res = await _http.PostAsJsonAsync("api/categories", category);
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> UpdateAsync(Category category)
    {
        var res = await _http.PutAsJsonAsync($"api/categories/{category.Id}", category);
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> DeleteAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/categories/{id}");
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
}