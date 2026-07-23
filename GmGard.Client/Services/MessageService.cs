using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class MessageService
{
    private readonly HttpClient _http;
    public MessageService(HttpClient http) => _http = http;

    public async Task<PagedResult<MessageDisplay>?> InboxAsync(int page = 1, bool unreadOnly = false)
    {
        try { return await _http.GetFromJsonAsync<PagedResult<MessageDisplay>>($"/api/Message/Inbox?pagenum={page}&unreadOnly={unreadOnly}"); }
        catch { return null; }
    }

    public async Task<PagedResult<MessageDisplay>?> OutboxAsync(int page = 1)
    {
        try { return await _http.GetFromJsonAsync<PagedResult<MessageDisplay>>($"/api/Message/Outbox?pagenum={page}"); }
        catch { return null; }
    }

    public async Task<MessageDetails?> ReadAsync(int id, bool markRead = false)
    {
        try { return await _http.GetFromJsonAsync<MessageDetails>($"/api/Message/Content?id={id}&markRead={markRead}"); }
        catch { return null; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { var r = await _http.DeleteAsync($"/api/Message/Delete?id={id}"); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<(bool ok, string? error)> SendAsync(SendMessageRequest req)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/api/Message/Send", req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<List<UserSuggestion>> SuggestUserAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2) return [];
        try
        {
            // Main-site endpoint: /api/Account/SuggestUser (legacy), app endpoint: /api/Account/SuggestUser too?
            // Fall back to main site via same origin
            var result = await _http.GetFromJsonAsync<List<UserSuggestion>>($"/api/Account/SuggestUser?name={Uri.EscapeDataString(name)}");
            return result ?? [];
        }
        catch { return []; }
    }
}
