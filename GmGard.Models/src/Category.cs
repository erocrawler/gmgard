using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GmGard.Models
{
    public class Category
    {
        [ScaffoldColumn(false)]
        public int CategoryID { get; set; }

        [Required, StringLength(100), Display(Name = "Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Category Description")]
        [MaxLength(256)]
        public string Description { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }

        public virtual ICollection<Topic> Topics { get; set; }

        public int? ParentCategoryID { get; set; }

        [InverseProperty("SubCategories"), ForeignKey("ParentCategoryID")]
        public Category ParentCategory { get; set; }

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