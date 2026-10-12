using System.Net.Http.Json;
namespace StoreClient;

public class UserApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    public async Task<List<UserInfo>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<UserInfo>>("api/users") ?? new();
    public async Task<string?> AddAsync(string username, string fullName,
                                        string password, string role)
    {
        var res = await _http.PostAsJsonAsync("api/users",
            new { username, fullName, password, role });
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
    public async Task<string?> DeleteAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/users/{id}");
        return res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync();
    }
}