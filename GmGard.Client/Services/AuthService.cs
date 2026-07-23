using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GmGard.Client.Models;
using Microsoft.AspNetCore.Components;

namespace GmGard.Client.Services;

/// <summary>
/// Cookie-based authentication service for the Blazor WASM client.
/// The HttpClient is configured with default credentials, so the ASP.NET Core
/// Identity cookie (.AspNetCore.Identity.Application) is sent automatically on
/// every request. Auth state is cached in-memory after the first check.
/// </summary>
public class AuthService
{
    private readonly HttpClient _http;
    private readonly NavigationManager _nav;

    // Cached auth state (null = unknown, not yet checked)
    private bool? _isAuthenticated;
    private CurrentUser? _user;

    /// <summary>
    /// Returns the cached authentication status without querying the server.
    /// Null if not yet checked.
    /// </summary>
    public bool? IsAuthenticatedCached => _isAuthenticated;

    /// <summary>URL to redirect to after a successful login.</summary>
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// Called after login/logout to refresh Blazor's auth state.
    /// Set by <see cref="CookieAuthenticationStateProvider"/> on startup.
    /// </summary>
    public Func<Task>? AuthStateChanged { get; set; }

    public AuthService(HttpClient http, NavigationManager nav)
    {
        _http = http;
        _nav = nav;
    }

    /// <summary>
    /// Checks if the user is authenticated. Caches the result after the first
    /// call. Use <paramref name="force"/> to re-query the server.
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync(bool force = false)
    {
        if (_isAuthenticated != null && !force)
            return _isAuthenticated.Value;

        try
        {
            var status = await _http.GetFromJsonAsync<AuthStatus>("/api/Account/IsAuthenticated");
            _isAuthenticated = status?.IsAuthenticated ?? false;
        }
        catch
        {
            _isAuthenticated = false;
        }
        return _isAuthenticated.Value;
    }

    /// <summary>
    /// Gets the current user profile. Returns null if not authenticated.
    /// Caches the result; use <paramref name="force"/> to refresh.
    /// </summary>
    public async Task<CurrentUser?> GetUserAsync(bool force = false)
    {
        if (_user != null && !force)
            return _user;

        if (!await IsAuthenticatedAsync(force))
        {
            _user = null;
            return null;
        }

        try
        {
            _user = await _http.GetFromJsonAsync<CurrentUser>("/api/Account/GetUser");
        }
        catch
        {
            _user = null;
        }
        return _user;
    }

    /// <summary>
    /// Attempts to log in. On success, refreshes cached auth state and user.
    /// </summary>
    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        LoginResult result;
        try
        {
            var response = await _http.PostAsJsonAsync("/api/Account/Login", request);
            result = await response.Content.ReadFromJsonAsync<LoginResult>() ?? new LoginResult();
        }
        catch
        {
            return new LoginResult { Errors = { "登录请求失败，请检查网络连接。" } };
        }

        if (result.Success && !result.Require2fa)
        {
            // Login succeeded — refresh cached state
            _isAuthenticated = true;
            await GetUserAsync(force: true);
            if (AuthStateChanged != null) await AuthStateChanged();
        }
        return result;
    }

    /// <summary>
    /// Logs out the current user and clears cached state.
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            await _http.PostAsync("/api/Account/LogOff", null);
        }
        catch
        {
            // Even if the server call fails, clear local state
        }
        _isAuthenticated = false;
        _user = null;
        if (AuthStateChanged != null) await AuthStateChanged();
    }

    /// <summary>
    /// Redirects to the login page, remembering the current URL for post-login
    /// redirect. Call from route guards or "requires auth" UI.
    /// </summary>
    public void RedirectToLogin(string? returnUrl = null)
    {
        RedirectUrl = returnUrl ?? _nav.ToBaseRelativePath(_nav.Uri);
        _nav.NavigateTo("login");
    }

    /// <summary>
    /// After a successful login, navigates to the remembered redirect URL or
    /// the home page.
    /// </summary>
    public void NavigateAfterLogin()
    {
        var target = RedirectUrl;
        RedirectUrl = null;
        _nav.NavigateTo(string.IsNullOrEmpty(target) ? "" : target);
    }
}
