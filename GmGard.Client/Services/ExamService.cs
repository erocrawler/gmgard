using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class ExamService
{
    private readonly HttpClient _http;
    public ExamService(HttpClient http) => _http = http;

    public string[] CurrentExamVersions() => ["201705", "201707"];

    public async Task<Exam?> GetExamAsync(string version)
    {
        try { return await _http.GetFromJsonAsync<Exam>($"/assets/audit-exam-{version}.json"); }
        catch { return null; }
    }

    public async Task<ExamSubmission?> GetDraftAsync(string version)
    {
        try { return await _http.GetFromJsonAsync<ExamSubmission>($"/api/AuditExam/Draft?version={version}"); }
        catch { return null; }
    }

    public async Task<bool> SaveDraftAsync(ExamSubmission draft)
    {
        try { var r = await _http.PutAsJsonAsync("/api/AuditExam/Draft", draft); return r.IsSuccessStatusCode; }
        catch { return false; }
    }

    public async Task<ExamResult?> SubmitAsync(ExamSubmission sub)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/api/AuditExam/Submit", sub);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ExamResult>();
        }
        catch { return null; }
    }

    public async Task<ExamResult?> GetResultAsync(string version)
    {
        try { return await _http.GetFromJsonAsync<ExamResult>($"/api/AuditExam/Result?version={version}"); }
        catch { return null; }
    }

    public async Task<List<ExamResult>?> GetAllResultsAsync()
    {
        try { return await _http.GetFromJsonAsync<List<ExamResult>>("/api/AuditExam/Results"); }
        catch { return null; }
    }

    public async Task<ExamResult?> GetResultForUserAsync(string user, string version)
    {
        try { return await _http.GetFromJsonAsync<ExamResult>($"/api/AuditExam/ResultForUser?version={version}&user={Uri.EscapeDataString(user)}"); }
        catch { return null; }
    }
}
