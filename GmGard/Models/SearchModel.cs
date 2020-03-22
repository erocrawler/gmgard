using GmGard.Extensions;
using GmGard.Services;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using X.PagedList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GmGard.Models
{
    public class SearchModel
    {
        public IEnumerable<int> CategoryIds
        {
            get { return _categoryIds; }
            set { _categoryIds = new SortedSet<int>(value); }
        }

        [Display(Name = "综合搜索"), MaxLength(120, ErrorMessage = "综合搜索不能超过120字符")]
        public string Query { get; set; }

        [Display(Name = "标题"), MaxLength(120, ErrorMessage = "标题搜索不能超过120字符")]
        public string Title { get; set; }

        [DefaultValue(false)]
        public bool TitleMatchAny { get; set; }

        [Display(Name = "标签"), MaxLength(120, ErrorMessage = "标签搜索不能超过120字符")]
        public string Tags { get; set; }

        [DefaultValue(false)]
        public bool TagsMatchAny { get; set; }

        public IEnumerable<int> TagIds
        {
            get { return _tagIds; }
            set { _tagIds = new SortedSet<int>(value); }
        }

        [DefaultValue(true)]
        public bool? Harmony { get; set; }

        public string Sort { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [BindNever]
        public string Author { get; set; }

        [BindNever]
        public string FavUser { get; set; }

        [BindNever]
        public int? CurrentCategory { get; set; }

        [BindNever]
        public IDictionary<int, long> CategoryItemCount { get; set; }

        public SortOptions SortOptions()
        {
            return new SortOptions(Sort, !string.IsNullOrWhiteSpace(Query));
        }

        public List<Tag> TagsSearched()
        {
            _tagsSearched = _tagsSearched.ToList();
            return _tagsSearched as List<Tag>;
        }

        private SortedSet<int> _categoryIds = new SortedSet<int>();
        private SortedSet<int> _tagIds = new SortedSet<int>();
        private IEnumerable<Tag> _tagsSearched = new List<Tag>();

        public CategoryOptionDisplay GetCategoryOptions()
        {
            return new CategoryOptionDisplay
            {
                SelectedIds = _categoryIds,
                CategoryItemCount = CategoryItemCount,
            };
        }

        public bool Cacheable()
        {
            return (new[] { Tags, Title, Author, FavUser, Query }).All(string.IsNullOrEmpty)
                && !StartDate.HasValue && !EndDate.HasValue;
        }

        public string CacheKey(int PageNumber, int PageCount)
        {
            var category = (CurrentCategory.HasValue ? CurrentCategory.ToString() : string.Empty) + '|' + string.Join(",", _categoryIds);
            return $"{PageNumber}|{PageCount}|{Harmony.ToString()}|{Sort}|{category}|{string.Join(",", _tagIds)}";
        }

        public SearchRouteDictionary ToRouteDictionary()
        {
            SearchRouteDictionary dict = new SearchRouteDictionary();
            foreach (var prop in GetType().GetProperties())
            {
                var key = prop.Name;
                object value = prop.GetValue(this);
                if (value == null)
                {
                    continue;
                }
                var never = prop.GetCustomAttributes(typeof(BindNeverAttribute), true);
                if (never != null && never.Length > 0)
                {
                    continue;
                }
                var defaultValue = prop.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                if (defaultValue != null && defaultValue.Length > 0 && (defaultValue[0] as DefaultValueAttribute).Value.Equals(value))
                {
                    continue;
                }
                if (value is IEnumerable && !(value is string))
                {
                    int index = 0;
                    foreach (object val in (IEnumerable)value)
                    {
                        if (val is string || val.GetType().IsPrimitive)
                        {
                            dict.Add(string.Format("{0}[{1}]", key, index), val);
                        }
                        else
                        {
                            var properties = val.GetType().GetProperties();
                            foreach (var propInfo in properties)
                            {
                                dict.Add(
                                    string.Format("{0}[{1}].{2}", key, index, propInfo.Name),
                                    propInfo.GetValue(val));
                            }
                        }
                        index++;
                    }
                }
                else
                {
                    dict.Add(prop.Name, value);
                }
            }
            return dict;
        }
    }

    public class SearchRouteDictionary : RouteValueDictionary
    {
        public SearchRouteDictionary Set(string key, object value)
        {
            if (this.ContainsKey(key))
            {
                this.Remove(key);
            }
            this.Add(key, value);
            return this;
        }

        public SearchRouteDictionary Merge(object values)
        {
            foreach (var entry in new RouteValueDictionary(values))
            {
                this[entry.Key] = entry.Value;
            }
            return this;
        }
    }

    public class SearchBlogResult
    {
        public IEnumerable<Tag> TagsSearched { get; set; }
        public IPagedList<Blog> Blogs { get; set; }
        public SearchModel SearchModel { get; set; }
        public bool HasError { get; set; }
        public string Error { get; set; }
    }
}