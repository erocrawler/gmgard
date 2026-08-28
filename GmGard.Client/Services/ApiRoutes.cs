namespace GmGard.Client.Services;

using GmGard.Client.Models;

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
        public static string Disable2Fa(bool reset) => Add($"{Base}/Disable2Fa", "reset", reset.ToString().ToLower());
        public const string ForgetClient = Base + "/ForgetClient";
        public const string GenerateRecoveryCodes = Base + "/GenerateRecoveryCodes";
        public const string TwoFactorAuth = Base + "/TwoFactorAuth";
        public const string RecoveryCode = Base + "/RecoveryCode";
        public const string IsAuthenticated = Base + "/IsAuthenticated";
        public const string GetUser = Base + "/GetUser";
        public const string Manage2Fa = Base + "/Manage2Fa";
        public const string Get2FaKeys = Base + "/Get2FaKeys";
        public const string Login = Base + "/Login";
        public const string LogOff = Base + "/LogOff";
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
        public const string Categories = Base + "/Category";
        public static string Category(int id) => Add(Categories, "id", id.ToString());
        public static string DeleteInvitationCode(string code, string reason, bool notice)
            => Add(InvitationCode, new Dictionary<string, string?>
            {
                ["code"] = code,
                ["reason"] = reason,
                ["notice"] = notice.ToString().ToLower()
            });
        public const string TitleCategoriesSave = Base + "/TitleCategories/Save";
    }

    // Bounty
    public static class Bounty
    {
        public const string Base = "/api/Bounty";
        public static string List(int page, BountyShowType showType, bool onlyMine, bool includeDeleted)
            => Add(Base + "/List", new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["showType"] = showType.ToString(),
                ["onlyMine"] = onlyMine.ToString().ToLower(),
                ["includeDeleted"] = includeDeleted.ToString().ToLower()
            });
        public static string My(int page, bool includeDeleted)
            => Add(Base + "/My", new Dictionary<string, string?>
            {
                ["page"] = page.ToString(),
                ["includeDeleted"] = includeDeleted.ToString().ToLower()
            });
        public static string Details(int id) => Add(Base + "/Details", "id", id.ToString());
        public const string Create = Base + "/Create";
        public const string UploadImages = Base + "/UploadImages";
        public const string Answer = Base + "/Answer";
        public const string Accept = Base + "/Accept";
        public static string Delete(int id) => Add(Base + "/Delete", "id", id.ToString());
        public static string DemoteAnswer(int answerId) => Add(Base + "/DemoteAnswer", "answerId", answerId.ToString());
        public static string PromoteComment(int postId) => Add(Base + "/PromoteComment", "postId", postId.ToString());
        public static string Close(int id) => Add(Base + "/Close", "id", id.ToString());
        public const string GetConfig = Base + "/GetConfig";
        public const string ReplyAnswer = Base + "/ReplyAnswer";
        public const string ReplyPost = Base + "/ReplyPost";
        public const string CommentBounty = Base + "/CommentBounty";
        public const string Report = Base + "/Report";
        public static string Search(string q, int page, BountyShowType showType, bool onlyMine, bool includeDeleted)
            => Add(Base + "/Search", new Dictionary<string, string?>
            {
                ["q"] = q,
                ["page"] = page.ToString(),
                ["showType"] = showType.ToString(),
                ["onlyMine"] = onlyMine.ToString().ToLower(),
                ["includeDeleted"] = includeDeleted.ToString().ToLower()
            });
    }

    // Raffle / Lottery
    public static class Raffle
    {
        public const string Base = "/api/Raffle";
        public static string Get(int id) => Add(Base, "id", id.ToString());
        public static string All(int page) => Add(Base + "/All", "page", page.ToString());
        public static string Draft(int id) => Add(Base + "/Draft", "id", id.ToString());
        public const string Create = Base;
    }

    // PunchIn
    public static class PunchIn
    {
        public const string Base = "/api/punchIn";
        public static string Cost(string date) => Add(Base + "/cost", "date", date);
        public const string Do = Base + "/do";
        public static string History(int year, int month) => Add(Base + "/history", new Dictionary<string, string?>
        {
            ["year"] = year.ToString(),
            ["month"] = month.ToString()
        });
    }

    // TitleHelper
    public static class TitleHelper
    {
        public const string Base = "/api/TitleHelper";
        public const string Categories = Base + "/Categories";
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
        public const string Base = "/api/AuditExam";
        public static string Draft(string version) => Add(Base + "/Draft", "version", version);
        public const string DraftPut = Base + "/Draft";
        public const string Submit = Base + "/Submit";
        public static string Result(string version) => Add(Base + "/Result", "version", version);
        public const string Results = Base + "/Results";
        public static string ResultForUser(string version, string user)
            => Add(Base + "/ResultForUser", new Dictionary<string, string?>
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

    // JS modules - cache busting version param (matches CKEDITOR.timestamp)
    public static class Js
    {
        // Bump this when JS assets change to bust CDN cache: ?v= timestamp
        public const string Version = "J9EB"; // sync with CKEDITOR.timestamp in ckeditor.js
        private static string V(string path) => $"{path}?v={Version}";
        public static string CkEditorEsm => V("./js/ck-editor.esm.js");
        public static string BountyContentEsm => V("./js/bounty-content.esm.js");
        public static string Enable2faMin => V("./js/enable2fa.min.js");
        public static string Enable2fa => V("./js/enable2fa.js");
        public static string QrCodeMin => V("./js/qrcode.min.js");
    }
}
