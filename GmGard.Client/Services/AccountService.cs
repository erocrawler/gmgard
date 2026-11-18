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

    private static FormUrlEncodedContent PasswordForm(string password) =>
        new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("password", password ?? "") });

    public async Task<(bool ok, string? error)> Disable2FaAsync(bool reset, string password)
    {
        try
        {
            var r = await _http.PostAsync(ApiRoutes.Account.Disable2Fa(reset), PasswordForm(password));
            if (r.IsSuccessStatusCode) return (true, null);
            return (false, r.StatusCode == System.Net.HttpStatusCode.BadRequest ? "密码错误" : null);
        }
        catch { return (false, null); }
    }

    /// <summary>已启用两步验证时,查看现有秘钥(用于在新设备上重新绑定验证器)需要密码。</summary>
    public async Task<(TwoFactorAuthSharedKey? key, string? error)> Show2FaKeysAsync(string password)
    {
        try
        {
            var r = await _http.PostAsync(ApiRoutes.Account.Show2FaKeys, PasswordForm(password));
            if (!r.IsSuccessStatusCode) return (null, r.StatusCode == System.Net.HttpStatusCode.BadRequest ? "密码错误" : "加载失败,请刷新重试");
            return (await r.Content.ReadFromJsonAsync<TwoFactorAuthSharedKey>(), null);
        }
        catch { return (null, "加载失败,请刷新重试"); }
    }

    public async Task<bool> ForgetClientAsync()
    {
        try { var r = await _http.PostAsync(ApiRoutes.Account.ForgetClient, null); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<(bool ok, string[]? codes, string? error)> GenerateRecoveryCodesAsync(string password)
    {
        try
        {
            var resp = await _http.PostAsync(ApiRoutes.Account.GenerateRecoveryCodes, PasswordForm(password));
            if (!resp.IsSuccessStatusCode) return (false, null, resp.StatusCode == System.Net.HttpStatusCode.BadRequest ? "密码错误" : await resp.Content.ReadAsStringAsync());
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
