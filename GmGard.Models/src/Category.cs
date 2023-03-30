using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GmGard.Models
{
    public class Category
    {
        [ScaffoldColumn(false)]
        public int CategoryID { get; set; }

        [Required, StringLength(100), Display(Name = "Name")]
        public string CategoryName { get; set; }

        [MaxLength(256)]
        public string Description { get; set; }

        // If true, link is not required for posts in this category.
        public bool LinkOptional { get; set; }
        // If true, posts in this category is not ranked.
        public bool DisableRanking { get; set; }
        // If true, ratings are disabled for posts in this category.
        public bool DisableRating { get; set; }
        // If true, posts from this category are hidden from home page.
        public bool HideFromHomePage { get; set; }

        [JsonIgnore]
        public virtual ICollection<Blog> Blogs { get; set; }

        [JsonIgnore]
        public virtual ICollection<Topic> Topics { get; set; }

        public int? ParentCategoryID { get; set; }

        [JsonIgnore]
        [InverseProperty("SubCategories"), ForeignKey("ParentCategoryID")]
        public Category ParentCategory { get; set; }

        [JsonIgnore]
        public ICollection<Category> SubCategories { get; set; }
    }

    public class CategoryTree
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<CategoryTree> SubCategories { get; set; }

        public static CategoryTree GenerateTree(Category cat)
        {
            var root = new CategoryTree { CategoryID = cat.CategoryID, CategoryName = cat.CategoryName, SubCategories = new List<CategoryTree>() };
            if (cat.SubCategories != null)
            {
                foreach (var subcat in cat.SubCategories)
                {
                    var node = GenerateTree(subcat);
                    root.SubCategories.Add(node);
                }
            }
            return root;
        }
    }
}