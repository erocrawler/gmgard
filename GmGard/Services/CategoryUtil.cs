using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GmGard.Services
{
    public class CategoryUtil : UtilityService
    {
        public CategoryUtil(BlogContext db, UsersContext udb, IMemoryCache cache) : base(db, udb, cache)
        {
        }

        public string CategoryInfo(int id)
        {
            var category = GetCategoryList().SingleOrDefault(c => c.CategoryID == id);
            return category.CategoryName;
        }

        public Category GetCategory(int id)
        {
            return GetCategoryList().SingleOrDefault(c => c.CategoryID == id);
        }

        public List<string> GetFullCategoryNames(int id)
        {
            var category = GetCategoryList().SingleOrDefault(c => c.CategoryID == id);
            var names = new List<string>();
            names.Insert(0, category.CategoryName);
            while (category.ParentCategoryID != null)
            {
                category = category.ParentCategory;
                names.Insert(0, category.CategoryName);
            }
            return names;
        }

        public int? GetParentCategoryId(int id)
        {
            var category = GetCategoryList().SingleOrDefault(c => c.CategoryID == id);
            if (category.ParentCategoryID != null)
            {
                return category.ParentCategoryID.Value;
            }
            return null;
        }

        public int GetAncestor(int id)
        {
            var category = GetCategoryList().SingleOrDefault(c => c.CategoryID == id);
            while (category.ParentCategoryID != null)
            {
                category = category.ParentCategory;
            }
            return category.CategoryID;
        }

        public List<int> GetParentCategories(int id)
        {
            var category = GetCategoryList().SingleOrDefault(c => c.CategoryID == id);
            var ParentCategories = new List<int>();
            while (category.ParentCategoryID != null)
            {
                ParentCategories.Insert(0, category.ParentCategoryID.Value);
                category = category.ParentCategory;
            }
            return ParentCategories;
        }

        public List<Category> GetCategoryList()
        {
            var cl = _cache.GetOrCreate<List<Category>>("~Categories", (_) => _db.Categories.ToList());
            return cl;
        }

        public List<SelectListItem> GetCategoryDropdown(int SelectedId = 0)
        {
            var categories = GetCategoryList();
            var list = new List<SelectListItem>(categories.Count + 1);
            Stack<Tuple<Category, int>> catStack = new Stack<Tuple<Category, int>>(categories.Where(c => c.ParentCategoryID == null)
                .OrderByDescending(c => c.CategoryID).Select(c => new Tuple<Category, int>(c, 0)));
            list.Add(new SelectListItem());
            while (catStack.Count > 0)
            {
                var cat = catStack.Pop();
                var item = new SelectListItem();
                item.Value = cat.Item1.CategoryID.ToString();
                item.Text = cat.Item1.CategoryName;
                if (SelectedId == cat.Item1.CategoryID)
                {
                    item.Selected = true;
                }
                if (cat.Item2 > 0)
                {
                    item.Text = "└".PadLeft(cat.Item2, '　') + item.Text; //全角空格
                }
                if (cat.Item1.SubCategories != null)
                {
                    foreach (var subcat in cat.Item1.SubCategories)
                    {
                        catStack.Push(new Tuple<Category, int>(subcat, cat.Item2 + 1));
                    }
                }
                list.Add(item);
            }
            return list;
        }

        public List<int> GetCategoryWithSubcategories(int id)
        {
            var result = new List<int>();
            var categories = GetCategoryList();
            var category = categories.SingleOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return result;
            }
            result = category.SubCategories.FlattenCategories().Select(c => c.CategoryID).ToList();
            result.Add(id);
            return result;
        }

        public List<int> GetCategoryWithSubcategories(IEnumerable<int> ids)
        {
            return new SortedSet<int>(ids).Aggregate(new List<int>(), (l, id) => { l.AddRange(GetCategoryWithSubcategories(id)); return l; });
        }

        public List<CategoryTree> GetCategoryTrees()
        {
            var categories = GetCategoryList();
            var list = new List<CategoryTree>();
            foreach (var cat in categories.Where(c => c.ParentCategoryID == null))
            {
                list.Add(CategoryTree.GenerateTree(cat));
            }
            return list;
        }
    }

    static class CategoryExtension
    {
        public static IEnumerable<Category> FlattenCategories(this IEnumerable<Category> e)
        {
            if (e == null)
            {
                return Enumerable.Empty<Category>();
            }
            return e.SelectMany(c => c.SubCategories.FlattenCategories()).Concat(e);
        }
    }
}