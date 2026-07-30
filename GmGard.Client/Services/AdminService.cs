using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class AdminService
{
    private readonly HttpClient _http;
    public AdminService(HttpClient http) => _http = http;

    // Invitation codes
    public async Task<InvitationCodeResponse?> GetInvitationCodeAsync(InvitationCodeRequest req)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Admin.InvitationCode, req);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<InvitationCodeResponse>();
        }
        catch { return null; }
    }

    public async Task<bool> DeleteInvitationCodeAsync(string code, string reason, bool notice)
    {
        try
        {
            var r = await _http.DeleteAsync(ApiRoutes.Admin.DeleteInvitationCode(code, reason, notice));
            return r.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // Categories
    public async Task<List<CategoryAdmin>?> GetCategoriesAsync()
    {
        try { return await _http.GetFromJsonAsync<List<CategoryAdmin>>(ApiRoutes.Admin.Categories); }
        catch { return null; }
    }

    public async Task<int?> UpdateCategoryAsync(CategoryAdmin cat)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Admin.Categories, cat);
            if (!resp.IsSuccessStatusCode) return null;
            var body = await resp.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return body?["id"];
        }
        catch { return null; }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try { var r = await _http.DeleteAsync(ApiRoutes.Admin.Category(id)); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    // Raffles
    public async Task<PagedResult<RaffleConfig>?> AllRafflesAsync(int page = 1)
    {
        try { return await _http.GetFromJsonAsync<PagedResult<RaffleConfig>>(ApiRoutes.Raffle.All(page)); }
        catch { return null; }
    }

    public async Task<bool> AddRaffleAsync(RaffleConfig cfg)
    {
        try { var r = await _http.PutAsJsonAsync(ApiRoutes.Raffle.Create, cfg); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> UpdateRaffleAsync(RaffleConfig cfg)
    {
        try { var r = await _http.PatchAsJsonAsync(ApiRoutes.Raffle.Create, cfg); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<DraftResult?> DraftRaffleAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<DraftResult>(ApiRoutes.Raffle.Draft(id)); }
        catch { return null; }
    }
}
