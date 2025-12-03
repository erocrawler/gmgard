using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class DLsiteSearchService
{
    private readonly HttpClient _http;

    public DLsiteSearchService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<DLsite>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<DLsite>();

        try
        {
            var results = await _http.GetFromJsonAsync<List<DLsite>>($"/api/DLsite/Search?query={Uri.EscapeDataString(query)}");
            return results ?? new List<DLsite>();
        }
        catch
        {
            return new List<DLsite>();
        }
    }
}
