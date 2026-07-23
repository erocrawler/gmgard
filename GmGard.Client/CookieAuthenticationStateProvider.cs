using System.Security.Claims;
using System.Threading.Tasks;
using GmGard.Client.Models;
using GmGard.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace GmGard.Client;

/// <summary>
/// Bridges the cookie-based AuthService with Blazor's authorization system.
/// <see cref="GetAuthenticationStateAsync"/> is called by AuthorizeRouteView
/// on every navigation to determine whether the user can access a route.
/// </summary>
public class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthService _authService;
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public CookieAuthenticationStateProvider(AuthService authService)
    {
        _authService = authService;
        _authService.AuthStateChanged = async () =>
        {
            await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged();
        };
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await _authService.GetUserAsync();
        _cachedUser = ToClaimsPrincipal(user);
        return new AuthenticationState(_cachedUser);
    }

    /// <summary>
    /// Notifies Blazor that auth state has changed (e.g. after login/logout).
    /// Call this after a successful login or logout.
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        var authState = Task.FromResult(new AuthenticationState(_cachedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private static ClaimsPrincipal ToClaimsPrincipal(CurrentUser? user)
    {
        if (user == null)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.GivenName, user.NickName),
        };
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "Identity.Application");
        return new ClaimsPrincipal(identity);
    }
}
