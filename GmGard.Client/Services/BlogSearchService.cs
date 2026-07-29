using System.Net.Http.Json;
using GmGard.Client.Models;

namespace GmGard.Client.Services;

public class BlogSearchService
{
    private readonly HttpClient _http;

    public BlogSearchService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<BlogPreview>> SearchAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 5)
            return new List<BlogPreview>();

        try
        {
            var results = await _http.GetFromJsonAsync<List<BlogPreview>>(ApiRoutes.Blog.SearchTitle(title));
            return results ?? new List<BlogPreview>();
        }
        catch
        {
            return new List<BlogPreview>();
        }
    }
}
