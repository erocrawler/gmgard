using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class RaffleService
{
    private readonly HttpClient _http;
    public RaffleService(HttpClient http) => _http = http;

    public async Task<RaffleConfig?> GetAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<RaffleConfig>(ApiRoutes.Raffle.Get(id)); }
        catch { return null; }
    }

    public async Task<bool> BuyAsync(int id)
    {
        try { var r = await _http.PostAsync(ApiRoutes.Raffle.Get(id), null); return r.IsSuccessStatusCode; }
        catch { return false; }
    }
}
