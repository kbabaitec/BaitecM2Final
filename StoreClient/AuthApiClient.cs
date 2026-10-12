using System.Net.Http.Json;

namespace StoreClient;

public class AuthApiClient
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(ApiConfig.BaseUrl) };

    public async Task<(UserInfo? User, string? Error)> LoginAsync(string username, string password)
    {
        var res = await _http.PostAsJsonAsync("api/auth/login", new { username, password });

        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<UserInfo>(), null);

        return (null, await res.Content.ReadAsStringAsync());
    }
}
