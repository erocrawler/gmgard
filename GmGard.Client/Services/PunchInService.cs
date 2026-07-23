using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class PunchInService
{
    private readonly HttpClient _http;

    public PunchInService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CurrentUser?> GetUserAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<CurrentUser>("/api/Account/GetUser");
        }
        catch
        {
            return null;
        }
    }

    public async Task<PunchInCost?> GetCostAsync(DateOnly date)
    {
        try
        {
            return await _http.GetFromJsonAsync<PunchInCost>($"/api/punchIn/cost?date={date:yyyy-MM-dd}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<PunchInResult?> DoPunchInAsync(DateOnly date, bool useTicket)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/punchIn/do", new
            {
                date = date.ToString("yyyy-MM-dd"),
                useTicket
            });
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<PunchInResult>();
            // Server may return error body with success=false
            try { return await response.Content.ReadFromJsonAsync<PunchInResult>(); }
            catch { return null; }
        }
        catch
        {
            return null;
        }
    }

    public async Task<PunchInHistoryResponse?> GetHistoryAsync(int year, int month)
    {
        try
        {
            return await _http.GetFromJsonAsync<PunchInHistoryResponse>(
                $"/api/punchIn/history?year={year}&month={month}");
        }
        catch
        {
            return null;
        }
    }
}
