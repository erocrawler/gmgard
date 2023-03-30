using GmGard.Filters;
using GmGard.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace GmGard.Models
{
    public class BlogDisplay
    {
        public Blog blog { get; set; }
        public IEnumerable<Tag> tag { get; set; }
    }

    public class BlogDetailDisplay : BlogDisplay
    {
        public string AuthorDesc { get; set; }
        public BlogOption Option { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsFavorite { get; set; }
        public Category Category { get; set; }
    }

    public class BlogEdit
    {
        [Required(ErrorMessage = "请输入标题"), StringLength(120, ErrorMessage = "标题不得超过120个字符"), Display(Name = "标题")]
        public string BlogTitle { get; set; }

        [Required(ErrorMessage = "请输入内容"), StringLength(int.MaxValue), Display(Name = "内容"), DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [MaxLength(512)]
        public string ImagePath { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public bool IsLocalImg { get; set; }

        [Required(ErrorMessage = "请添加至少一个标签"), ValidateBlogTags(10)]
        public string BlogTags { get; set; }

        public BlogLink[] BlogLinks { get; set; }

        public int? HanGroupID { get; set; }

        public BlogOption Option { get; set; }

        public BlogEdit(string[] tags, Blog blog)
        {
            BlogTitle = blog.BlogTitle;
            Content = blog.Content;
            ImagePath = blog.ImagePath;
            CategoryID = blog.CategoryID;
            IsLocalImg = blog.IsLocalImg;
            if (tags != null && tags.Length != 0)
                BlogTags = string.Join(", ", tags);
            BlogLinks = BlogHelper.GetBlogLink(blog.Links);
            Option = blog.option;
        }

        public BlogEdit()
        {
        }
    }

    public class EditTagModel
    {
        public int BlogID { get; set; }
        public string Author { get; set; }
        public IEnumerable<TagsInBlog> TagsInBlog { get; set; }
        public IEnumerable<TagHistory> TagHistories { get; set; }
        public IDictionary<string, string> NickNames { get; set; }
        public IEnumerable<int> BlackListTagIDs { get; set; }

        public EditTagModel(BlogUtil util, BlogContext db, int id, string author, IEnumerable<int> BlackListTags)
        {
            BlogID = id;
            Author = author;
            TagsInBlog = db.TagsInBlogs.Include("tag").Where(tib => tib.BlogID == id);
            TagHistories = db.TagHistories.Where(th => th.BlogID == id);
            NickNames = util.GetNickNames(TagsInBlog.Select(tib => tib.AddBy).Where(b => b != null));
            BlackListTagIDs = BlackListTags;
        }
    }

    public class BlogLink
    {
        public string name { get; set; }
        public string url { get; set; }
        public string pass { get; set; }
    }

    public class BlogRatingDisplay
    {
        public int BlogId { get; set; }
        public int Total => CountByRating.Sum(kvp => kvp.Key * kvp.Value);
        public int Count => CountByRating.Values.Sum();
        public IDictionary<int, int> CountByRating { get; set; } = new Dictionary<int, int>();
        public double Average { get { return Count > 0 ? Total / (double)Count : 0; } }

        public override string ToString()
        {
            if (Count == 0)
                return "还没有人评分";
            return string.Format("{2}分（平均{0:0.##}），（共{1}次评分）", Average, Count, Total);
        }
    }
}