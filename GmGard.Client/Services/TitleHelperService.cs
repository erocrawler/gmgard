using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class TitleHelperService
{
    private readonly HttpClient _http;
    public TitleHelperService(HttpClient http) => _http = http;

    static Func<string, string> MakeFormatter(CategoryField f)
    {
        var ft = f.FormatType ?? "Bracket";
        return ft switch
        {
            "Bracket" => v => $"[{v}]",
            "RoundBracket" => v => $"({v})",
            "Space" => v => $" {v} ",
            "By" => v => $"(by {v})",
            "None" => v => v,
            "Custom" => string.IsNullOrEmpty(f.FormatTemplate)
                ? (Func<string, string>)(v => $"[{v}]")
                : f.FormatTemplate!.Contains("{0}")
                    ? v => string.Format(f.FormatTemplate!, v)
                    : _ => f.FormatTemplate!,
            _ => v => $"[{v}]"
        };
    }

    public async Task<List<TitleCategory>> GetCategoriesAsync()
    {
        // API returns List<TitleHelperCategory> which JSON shape matches Client TitleCategory exactly
        // So we deserialize directly into TitleCategory, then attach Format delegates
        var categories = await _http.GetFromJsonAsync<List<TitleCategory>>("/api/TitleHelper/Categories");
        if (categories == null) return new();
        foreach (var c in categories)
        {
            foreach (var f in c.Fields) f.Format = MakeFormatter(f);
        }
        return categories;
    }

    // Admin-only save - uses same shape as server TitleCategoriesConfig
    public async Task SaveAllAsync(List<TitleCategory> categories)
    {
        var payload = new { Categories = categories.Select(c => new TitleCategory
        {
            Id = c.Id,
            Name = c.Name,
            Comment = c.Comment,
            Fields = [.. c.Fields.Select(f => new CategoryField
            {
                Name = f.Name,
                Required = f.Required,
                Fixed = f.Fixed,
                Default = f.Default,
                    FormatType = f.FormatType ?? "Bracket",
                    FormatTemplate = f.FormatTemplate,
                    Hint = f.Hint
                })]
            }).ToList()
        };
        var resp = await _http.PostAsJsonAsync("/api/Admin/TitleCategories/Save", payload);
        resp.EnsureSuccessStatusCode();
    }
}
