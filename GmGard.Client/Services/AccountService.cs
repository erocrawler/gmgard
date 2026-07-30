using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class AccountService
{
    private readonly HttpClient _http;
    public AccountService(HttpClient http) => _http = http;

    public async Task<TwoFactorAuthenticationModel?> Get2FaDataAsync()
    {
        try { return await _http.GetFromJsonAsync<TwoFactorAuthenticationModel>(ApiRoutes.Account.Manage2Fa); }
        catch { return null; }
    }

    public async Task<TwoFactorAuthSharedKey?> Get2FaKeysAsync()
    {
        try { return await _http.GetFromJsonAsync<TwoFactorAuthSharedKey>(ApiRoutes.Account.Get2FaKeys); }
        catch { return null; }
    }

    public async Task<(bool ok, string[]? codes, string? error)> Enable2FaAsync(string code)
    {
        try
        {
            var resp = await _http.PostAsync(ApiRoutes.Account.Enable2Fa(code), null);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                return (false, null, err);
            }
            // Server returns JSON array of codes or empty body
            var str = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(str)) return (true, null, null);
            try
            {
                var codes = System.Text.Json.JsonSerializer.Deserialize<string[]>(str);
                return (true, codes, null);
            }
            catch { return (true, null, null); }
        }
        catch (Exception ex) { return (false, null, ex.Message); }
    }

    public async Task<bool> Disable2FaAsync(bool reset = false)
    {
        try { var r = await _http.PostAsync(ApiRoutes.Account.Disable2Fa(reset), null); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<bool> ForgetClientAsync()
    {
        try { var r = await _http.PostAsync(ApiRoutes.Account.ForgetClient, null); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<(bool ok, string[]? codes, string? error)> GenerateRecoveryCodesAsync()
    {
        try
        {
            var resp = await _http.PostAsync(ApiRoutes.Account.GenerateRecoveryCodes, null);
            if (!resp.IsSuccessStatusCode) return (false, null, await resp.Content.ReadAsStringAsync());
            var codes = await resp.Content.ReadFromJsonAsync<string[]>();
            return (true, codes, null);
        }
        catch (Exception ex) { return (false, null, ex.Message); }
    }

    public async Task<LoginResult> TwoFactorLoginAsync(bool rememberMe, bool rememberMachine, string code)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Account.TwoFactorAuth, new
            {
                rememberMe,
                rememberMachine,
                twoFactorCode = code
            });
            var result = await resp.Content.ReadFromJsonAsync<LoginResult>() ?? new();
            return result;
        }
        catch { return new LoginResult { Errors = { "请求失败" } }; }
    }

    public async Task<LoginResult> RecoveryCodeLoginAsync(string code)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Account.RecoveryCode, new { recoveryCode = code });
            var result = await resp.Content.ReadFromJsonAsync<LoginResult>() ?? new();
            return result;
        }
        catch { return new LoginResult { Errors = { "请求失败" } }; }
    }
}
