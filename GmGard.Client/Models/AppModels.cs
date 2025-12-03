using System;

namespace GmGard.Client.Models;

public class BlogPreview
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Brief { get; set; } = string.Empty;
    public string ThumbUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public User? Author { get; set; }
}

public class User
{
    public string Name { get; set; } = string.Empty;
}

public class DLsite
{
    public string RjCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Circle { get; set; } = string.Empty;
    public string CircleUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
