using System.Net.Http.Json;
namespace StoreClient;

public class DashboardApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };
    public async Task<DashboardInfo?> GetAsync() =>
        await _http.GetFromJsonAsync<DashboardInfo>("api/dashboard");
}