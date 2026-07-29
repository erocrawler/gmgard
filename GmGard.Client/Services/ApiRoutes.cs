namespace GmGard.Client.Services;

/// <summary>
/// Typed route builder – client-side equivalent of MVC Url.Action / Url.Route.
/// Centralizes API paths so we don't scatter "/api/..." strings.
/// Uses Uri.EscapeDataString internally but via Add helper to avoid manual concat bugs (like #hashtag bug in MessageController).
/// Prefer this over raw string interpolation; if Microsoft.AspNetCore.WebUtilities.QueryHelpers is available, you can swap impl.
/// </summary>
public static class ApiRoutes
{
    private static string Add(string path, string name, string? value)
    {
        if (value == null) return path;
        var sep = path.Contains('?') ? "&" : "?";
        return $"{path}{sep}{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
    }
    private static string Add(string path, IDictionary<string, string?> qs)
    {
        foreach (var kv in qs)
        {
            if (kv.Value == null) continue;
            path = Add(path, kv.Key, kv.Value);
        }
        return path;
    }
    // base helpers
    private static string Q(string path, IDictionary<string, string?>? qs)
        => qs == null ? path : Add(path, qs);

    // Account
    public static class Account
    {
        public const string Base = "/api/Account";
        public static string SuggestUser(string name) => Add($"{Base}/SuggestUser", "name", name);
        public static string Enable2Fa(string code) => Add($"{Base}/Enable2Fa", "code", code);
    }

    // Message
    public static class Message
    {
        public const string Base = "/api/Message";
        public static string Inbox(int page = 1, bool unreadOnly = false)
            => Add(Base + "/Inbox", new Dictionary<string, string?>
            {
                ["pagenum"] = page.ToString(),
                ["unreadOnly"] = unreadOnly.ToString().ToLower()
            });
        public static string Outbox(int page = 1) => Add(Base + "/Outbox", "pagenum", page.ToString());
        public static string Content(int id, bool markRead = false) => Add(Base + "/Content", new Dictionary<string, string?>
        {
            ["id"] = id.ToString(),
            ["markRead"] = markRead.ToString().ToLower()
        });
        public static string Delete(int id) => Add(Base + "/Delete", "id", id.ToString());
        public const string Send = Base + "/Send";
    }

    // Admin
    public static class Admin
    {
        public const string Base = "/api/Admin";
        public const string InvitationCode = Base + "/InvitationCode";
        public static string DeleteInvitationCode(string code, string reason, bool notice)
            => Add(InvitationCode, new Dictionary<string, string?>
            {
                ["code"] = code,
                ["reason"] = reason,
                ["notice"] = notice.ToString().ToLower()
            });
    }

    // Blog / Topics / Search etc.
    public static class Blog
    {
        public static string SearchTitle(string title) => Add("/api/Blog/SearchTitle", "title", title);
    }
    public static class DLsite
    {
        public static string Search(string query) => Add("/api/DLsite/Search", "query", query);
    }
    public static class AuditExam
    {
        public static string ResultForUser(string version, string user)
            => Add("/api/AuditExam/ResultForUser", new Dictionary<string, string?>
            {
                ["version"] = version,
                ["user"] = user
            });
    }

    public static class Blazor
    {
        // Pure relative - base href = /app/, so NavigationManager.NavigateTo("bounty/123") => /app/bounty/123
        public static string Bounty(int id) => $"bounty/{id}";
        public static string BountyPost(int id, int answerId) => $"bounty/{id}#postcontent{answerId}";
    }
}
