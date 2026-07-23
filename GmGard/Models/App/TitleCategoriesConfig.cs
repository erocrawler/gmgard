using System.Collections.Generic;

namespace GmGard.Models.App
{
    // Stored in App_Data/TitleCategories.json - editable via Admin API
    // No Dto suffix needed: all classes in Models.App are DTOs by convention
    public class TitleCategoriesConfig
    {
        public List<TitleHelperCategory> Categories { get; set; } = new();
    }

    public class TitleHelperCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<TitleHelperField> Fields { get; set; } = new();
    }

    public class TitleHelperField
    {
        public string Name { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool Fixed { get; set; }
        public string? Default { get; set; }
        /// <summary>
        /// Format type: Bracket [x], RoundBracket (x), Space " x ", By "(by x)", Custom template "[提取动画]" or "(by {0})", None raw
        /// </summary>
        public string FormatType { get; set; } = "Bracket";
        public string? FormatTemplate { get; set; }
        public string? Hint { get; set; }
    }
}
