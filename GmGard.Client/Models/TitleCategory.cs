namespace GmGard.Client.Models;

/// <summary>
/// Title helper specific category with fields for formatting titles
/// This is different from the database Category model
/// </summary>
public class TitleCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CategoryField> Fields { get; set; } = new();
    public string Comment { get; set; } = string.Empty;
}

public class CategoryField
{
    public string Name { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool Fixed { get; set; }
    public string? Default { get; set; }
    public Func<string, string>? Format { get; set; }
    public string? Hint { get; set; }
    public string? FormatType { get; set; }
    public string? FormatTemplate { get; set; }
}
