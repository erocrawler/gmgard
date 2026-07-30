using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class BountyService
{
    private readonly HttpClient _http;
    public BountyService(HttpClient http) => _http = http;

    public async Task<BountyPaged<BountyPreview>?> ListAsync(BountyShowType showType, int page = 1, bool onlyMine = false, bool includeDeleted = false)
    {
        try
        {
            var url = ApiRoutes.Bounty.List(page, showType, onlyMine, includeDeleted);
            return await _http.GetFromJsonAsync<BountyPaged<BountyPreview>>(url);
        }
        catch
        {
            return null;
        }
    }

    public async Task<BountyPaged<BountyPreview>?> MyAsync(int page = 1, bool includeDeleted = false, bool onlyMine = true)
    {
        try
        {
            return await _http.GetFromJsonAsync<BountyPaged<BountyPreview>>(ApiRoutes.Bounty.My(page, includeDeleted));
        }
        catch { return null; }
    }

    public async Task<BountyDetail?> GetAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<BountyDetail>(ApiRoutes.Bounty.Details(id));
        }
        catch { return null; }
    }

    // New buffered overload to avoid _blazorFilesById lifetime issue
    public async Task<BountyCreateResult> CreateAsyncBuffered(string title, string content, int prize, int helpfulReward, string[]? imageUrls, List<GmGard.Client.Components.BufferedImage>? bufferedFiles = null)
    {
        try
        {
            HttpResponseMessage resp;
            if (bufferedFiles != null && bufferedFiles.Count > 0)
            {
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(title.Trim()), "Title");
                form.Add(new StringContent(content.Trim()), "Content");
                form.Add(new StringContent(prize.ToString()), "Prize");
                form.Add(new StringContent(helpfulReward.ToString()), "HelpfulReward");
                form.Add(new StringContent(imageUrls != null && imageUrls.Length > 0 ? System.Text.Json.JsonSerializer.Serialize(imageUrls) : "[]"), "ImageUrls");
                foreach (var b in bufferedFiles)
                {
                    var sc = new ByteArrayContent(b.Data);
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(b.ContentType);
                    form.Add(sc, "Files", b.FileName);
                }
                resp = await _http.PostAsync(ApiRoutes.Bounty.Create, form);
            }
            else
            {
                var req = new CreateBountyRequestClient
                {
                    Title = title.Trim(),
                    Content = content.Trim(),
                    Prize = prize,
                    HelpfulReward = helpfulReward,
                    ImageUrls = imageUrls != null && imageUrls.Length > 0 ? System.Text.Json.JsonSerializer.Serialize(imageUrls) : "[]"
                };
                resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.Create, req);
            }
            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadFromJsonAsync<CreateBountyResponse>();
                return new BountyCreateResult { Success = true, Id = body?.Id, TotalCost = body?.TotalCost, Remaining = body?.Remaining };
            }
            else
            {
                var txt = await resp.Content.ReadAsStringAsync();
                string err;
                try
                {
                    var jo = System.Text.Json.JsonDocument.Parse(txt);
                    if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt;
                    else err = txt;
                }
                catch { err = txt; }
                return new BountyCreateResult { Success = false, Error = err };
            }
        }
        catch (Exception ex)
        {
            return new BountyCreateResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<BountyCreateResult> CreateAsync(string title, string content, int prize, int helpfulReward, string[]? imageUrls, List<Microsoft.AspNetCore.Components.Forms.IBrowserFile>? files = null)
    {
        try
        {
            HttpResponseMessage resp;
            if (files != null && files.Count > 0)
            {
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(title.Trim()), "Title");
                form.Add(new StringContent(content.Trim()), "Content");
                form.Add(new StringContent(prize.ToString()), "Prize");
                form.Add(new StringContent(helpfulReward.ToString()), "HelpfulReward");
                form.Add(new StringContent(imageUrls != null && imageUrls.Length > 0 ? System.Text.Json.JsonSerializer.Serialize(imageUrls) : "[]"), "ImageUrls");
                foreach (var f in files)
                {
                    Stream s;
                    try { s = f.OpenReadStream(10 * 1024 * 1024); }
                    catch (Exception ex) when (ex.Message.Contains("_blazorFilesById") || ex.Message.Contains("File"))
                    {
                        // If file ref already expired, skip it gracefully -> will be caught as empty upload on server
                        continue;
                    }
                    var streamContent = new StreamContent(s);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(f.ContentType);
                    form.Add(streamContent, "Files", f.Name);
                }
                resp = await _http.PostAsync(ApiRoutes.Bounty.Create, form);
            }
            else
            {
                var req = new CreateBountyRequestClient
                {
                    Title = title.Trim(),
                    Content = content.Trim(),
                    Prize = prize,
                    HelpfulReward = helpfulReward,
                    ImageUrls = imageUrls != null && imageUrls.Length > 0 ? System.Text.Json.JsonSerializer.Serialize(imageUrls) : "[]"
                };
                resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.Create, req);
            }

            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadFromJsonAsync<CreateBountyResponse>();
                return new BountyCreateResult { Success = true, Id = body?.Id, TotalCost = body?.TotalCost, Remaining = body?.Remaining };
            }
            else
            {
                var txt = await resp.Content.ReadAsStringAsync();
                string err;
                try
                {
                    var jo = System.Text.Json.JsonDocument.Parse(txt);
                    if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt;
                    else err = txt;
                }
                catch { err = txt; }
                return new BountyCreateResult { Success = false, Error = err };
            }
        }
        catch (Exception ex)
        {
            return new BountyCreateResult { Success = false, Error = ex.Message };
        }
    }

    // Overload for backward compat
    public Task<BountyCreateResult> CreateAsync(string title, string content, int prize, int helpfulReward, string[]? imageUrls) => CreateAsync(title, content, prize, helpfulReward, imageUrls, null);

    public async Task<string[]?> UploadImagesAsync(List<Microsoft.AspNetCore.Components.Forms.IBrowserFile> files)
    {
        try
        {
            var form = new MultipartFormDataContent();
            foreach (var f in files)
            {
                var sc = new StreamContent(f.OpenReadStream(10 * 1024 * 1024));
                sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(f.ContentType);
                form.Add(sc, "Files", f.Name);
            }
            var resp = await _http.PostAsync(ApiRoutes.Bounty.UploadImages, form);
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadFromJsonAsync<UploadResponse>();
            return json?.Urls;
        }
        catch { return null; }
    }

    private class UploadResponse { public string[]? Urls { get; set; } }

    public async Task<BountyAnswerResult> AnswerAsyncBuffered(int bountyId, string content, string? imageUrl, List<GmGard.Client.Components.BufferedImage>? bufferedFiles = null)
    {
        try
        {
            HttpResponseMessage resp;
            if (bufferedFiles != null && bufferedFiles.Count > 0)
            {
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(bountyId.ToString()), "BountyId");
                form.Add(new StringContent(content.Trim()), "Content");
                if (!string.IsNullOrEmpty(imageUrl)) form.Add(new StringContent(imageUrl), "ImageUrl");
                foreach (var b in bufferedFiles)
                {
                    var sc = new ByteArrayContent(b.Data);
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(b.ContentType);
                    form.Add(sc, "Files", b.FileName);
                }
                System.Console.WriteLine($"[BountyService] Posting buffered multipart Answer bountyId={bountyId} files={bufferedFiles.Count} contentLen={content?.Length}");
                resp = await _http.PostAsync(ApiRoutes.Bounty.Answer, form);
            }
            else
            {
                System.Console.WriteLine($"[BountyService] Posting JSON Answer bountyId={bountyId} no files");
                var req = new CreateAnswerRequestClient { BountyId = bountyId, Content = content.Trim(), ImageUrl = imageUrl };
                resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.Answer, req);
            }
            var respTxt = await resp.Content.ReadAsStringAsync();
            System.Console.WriteLine($"[BountyService] Answer response {(int)resp.StatusCode}: {respTxt}");
            if (resp.IsSuccessStatusCode)
            {
                try
                {
                    var jo = System.Text.Json.JsonDocument.Parse(respTxt);
                    int? aid = null;
                    if (jo.RootElement.TryGetProperty("answerId", out var el) && el.TryGetInt32(out var v)) aid = v;
                    return new BountyAnswerResult { Success = true, AnswerId = aid };
                }
                catch { return new BountyAnswerResult { Success = true }; }
            }
            string err = respTxt;
            try { var jo2 = System.Text.Json.JsonDocument.Parse(respTxt); if (jo2.RootElement.TryGetProperty("error", out var e2)) err = e2.GetString() ?? respTxt; } catch { }
            return new BountyAnswerResult { Success = false, Error = err };
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[BountyService] Answer exception: {ex}");
            return new BountyAnswerResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<BountyAnswerResult> AnswerAsync(int bountyId, string content, string? imageUrl, List<Microsoft.AspNetCore.Components.Forms.IBrowserFile>? files = null)
    {
        try
        {
            HttpResponseMessage resp;
            if (files != null && files.Count > 0)
            {
                var form = new MultipartFormDataContent();
                form.Add(new StringContent(bountyId.ToString()), "BountyId");
                form.Add(new StringContent(content.Trim()), "Content");
                if (!string.IsNullOrEmpty(imageUrl)) form.Add(new StringContent(imageUrl), "ImageUrl");
                foreach (var f in files)
                {
                    Stream s;
                    try { s = f.OpenReadStream(10 * 1024 * 1024); }
                    catch (Exception ex) when (ex.Message.Contains("_blazorFilesById") || ex.Message.Contains("File"))
                    {
                        continue;
                    }
                    var sc = new StreamContent(s);
                    sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(f.ContentType);
                    form.Add(sc, "Files", f.Name);
                }
                System.Console.WriteLine($"[BountyService] Posting multipart Answer bountyId={bountyId} files={files.Count} contentLen={content?.Length}");
                resp = await _http.PostAsync(ApiRoutes.Bounty.Answer, form);
            }
            else
            {
                System.Console.WriteLine($"[BountyService] Posting JSON Answer bountyId={bountyId} no files");
                var req = new CreateAnswerRequestClient { BountyId = bountyId, Content = content.Trim(), ImageUrl = imageUrl };
                resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.Answer, req);
            }

            var respTxt = await resp.Content.ReadAsStringAsync();
            System.Console.WriteLine($"[BountyService] Answer response {(int)resp.StatusCode}: {respTxt}");
            if (resp.IsSuccessStatusCode)
            {
                try
                {
                    var jo = System.Text.Json.JsonDocument.Parse(respTxt);
                    int? aid = null;
                    if (jo.RootElement.TryGetProperty("answerId", out var el) && el.TryGetInt32(out var v)) aid = v;
                    return new BountyAnswerResult { Success = true, AnswerId = aid };
                }
                catch { return new BountyAnswerResult { Success = true }; }
            }
            string err = respTxt;
            try
            {
                var jo2 = System.Text.Json.JsonDocument.Parse(respTxt);
                if (jo2.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? respTxt;
            }
            catch { }
            return new BountyAnswerResult { Success = false, Error = err };
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[BountyService] Answer exception: {ex}");
            return new BountyAnswerResult { Success = false, Error = ex.Message };
        }
    }

    public Task<BountyAnswerResult> AnswerAsync(int bountyId, string content, string? imageUrl) => AnswerAsync(bountyId, content, imageUrl, null);

    public async Task<(bool success, string? error)> AcceptAsync(int bountyId, int bestAnswerId, int[] helpfulIds)
    {
        try
        {
            var req = new AcceptRequestClient { BountyId = bountyId, BestAnswerId = bestAnswerId, HelpfulAnswerIds = helpfulIds };
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.Accept, req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var txt = await resp.Content.ReadAsStringAsync();
            string err;
            try
            {
                var jo = System.Text.Json.JsonDocument.Parse(txt);
                if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt;
                else err = txt;
            }
            catch { err = txt; }
            return (false, err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id)
    {
        try
        {
            var resp = await _http.PostAsync(ApiRoutes.Bounty.Delete(id), null);
            if (resp.IsSuccessStatusCode) return (true, null);
            var txt = await resp.Content.ReadAsStringAsync();
            string err;
            try
            {
                var jo = System.Text.Json.JsonDocument.Parse(txt);
                if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt;
                else err = txt;
            }
            catch { err = txt; }
            return (false, err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<CurrentUser?> GetCurrentUserAsync()
    {
        try { return await _http.GetFromJsonAsync<CurrentUser>(ApiRoutes.Account.GetUser); }
        catch { return null; }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<IsAuthDto>(ApiRoutes.Account.IsAuthenticated);
            return res?.IsAuthenticated == true;
        }
        catch { return false; }
    }

    private class IsAuthDto { public bool IsAuthenticated { get; set; } }

    public async Task<BountyConfigDto?> GetConfigAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<BountyConfigDto>(ApiRoutes.Bounty.GetConfig);
        }
        catch
        {
            // Fallback defaults if server not reachable or config endpoint missing
            return new BountyConfigDto();
        }
    }

    public async Task<(bool success, string? error)> ReplyAnswerAsync(int answerId, string content)
    {
        try
        {
            var req = new { AnswerId = answerId, Content = content.Trim() };
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.ReplyAnswer, req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var txt = await resp.Content.ReadAsStringAsync();
            string err;
            try { var jo = System.Text.Json.JsonDocument.Parse(txt); if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt; else err = txt; } catch { err = txt; }
            return (false, err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool success, string? error)> ReplyPostAsync(int postId, string content)
    {
        try
        {
            var req = new { PostId = postId, Content = content.Trim() };
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.ReplyPost, req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var txt = await resp.Content.ReadAsStringAsync();
            string err;
            try { var jo = System.Text.Json.JsonDocument.Parse(txt); if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt; else err = txt; } catch { err = txt; }
            return (false, err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // Bounty-level comments: Post IdType=Bounty, allowed even after closed/expired
    public async Task<(bool success, string? error)> CommentBountyAsync(int bountyId, string content)
    {
        try
        {
            var req = new { BountyId = bountyId, Content = content.Trim() };
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.CommentBounty, req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var txt = await resp.Content.ReadAsStringAsync();
            string err;
            try { var jo = System.Text.Json.JsonDocument.Parse(txt); if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt; else err = txt; } catch { err = txt; }
            return (false, err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool success, string? error, string? msg)> ReportAsync(int id, int itemType, int? postId, string content, string type)
    {
        try
        {
            var req = new { Id = id, ItemType = itemType, PostId = postId, MsgContent = content.Trim(), Type = type };
            var resp = await _http.PostAsJsonAsync(ApiRoutes.Bounty.Report, req);
            var txt = await resp.Content.ReadAsStringAsync();
            if (resp.IsSuccessStatusCode)
            {
                string m = "";
                try { var jo = System.Text.Json.JsonDocument.Parse(txt); if (jo.RootElement.TryGetProperty("msg", out var mm)) m = mm.GetString(); } catch {}
                return (true, null, m ?? "已成功报告。");
            }
            string err;
            try { var jo = System.Text.Json.JsonDocument.Parse(txt); if (jo.RootElement.TryGetProperty("error", out var e)) err = e.GetString() ?? txt; else err = txt; } catch { err = txt; }
            return (false, err, null);
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }
}
