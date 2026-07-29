using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class MessageService
{
    private readonly HttpClient _http;
    public MessageService(HttpClient http) => _http = http;

    public async Task<PagedResult<MessageDisplay>?> InboxAsync(int page = 1, bool unreadOnly = false)
    {
        try { return await _http.GetFromJsonAsync<PagedResult<MessageDisplay>>(ApiRoutes.Message.Inbox(page, unreadOnly)); }
        catch { return null; }
    }

    public async Task<PagedResult<MessageDisplay>?> OutboxAsync(int page = 1)
    {
        try { return await _http.GetFromJsonAsync<PagedResult<MessageDisplay>>(ApiRoutes.Message.Outbox(page)); }
        catch { return null; }
    }

    public async Task<MessageDetails?> ReadAsync(int id, bool markRead = false)
    {
        try { return await _http.GetFromJsonAsync<MessageDetails>(ApiRoutes.Message.Content(id, markRead)); }
        catch { return null; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { var r = await _http.DeleteAsync(ApiRoutes.Message.Delete(id)); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<(bool ok, string? error)> SendAsync(SendMessageRequest req)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Message.Send, req);
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
            var result = await _http.GetFromJsonAsync<List<UserSuggestion>>(ApiRoutes.Account.SuggestUser(name));
            return result ?? [];
        }
        catch { return []; }
    }
}
