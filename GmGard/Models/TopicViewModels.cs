using GmGard.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GmGard.Models
{
    public class TopicEdit
    {
        [Required(ErrorMessage = "请输入标题"), StringLength(80, ErrorMessage = "标题不得超过80个字符"), Display(Name = "标题")]
        public string TopicTitle { get; set; }

        [Required(ErrorMessage = "请输入内容"), DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [ValidateFile(4194304, ErrorMessage = "封面必须是图片，且不得超过4MB")]
        public IFormFile TopicImage { get; set; }

        [ValidateFile(4194304, ErrorMessage = "横幅必须是图片，且不得超过4MB")]
        public IFormFile TopicBanner { get; set; }

        public string ImagePath { get; set; }
        public bool IsLocalImg { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "请添加一个标签"), ValidateBlogTags(1, ErrorMessage = "专题只能有一个标签")]
        public string TagName { get; set; }

        [Required(ErrorMessage = "请添加资源"), ModelBinder(BinderType = typeof(Binding.StringSplitIntListModelBinder))]
        public List<int> BlogIDs { get; set; }

        public string BannerPath { get; set; }

        public IEnumerable<Blog> Blogs { get; set; }
        
        public TopicEdit(BlogContext db, Topic topic)
        {
            TopicTitle = topic.TopicTitle;
            Content = topic.Content;
            ImagePath = topic.ImagePath;
            CategoryID = topic.CategoryID;
            IsLocalImg = topic.isLocalImg;
            BannerPath = topic.BannerPath;
            TagName = topic.tag.TagName;
            Blogs = db.BlogsInTopics.Where(t => t.TopicID == topic.TopicID).OrderBy(t => t.BlogOrder).Select(t => t.blog).ToList();
            BlogIDs = Blogs.Select(t => t.BlogID).ToList();
        }

        public TopicEdit()
        {
        }

        public void LoadBlog(BlogContext db)
        {
            if (Blogs == null)
            {
                Blogs = db.Blogs.Where(b => BlogIDs.Contains(b.BlogID)).ToList().OrderBy(b => BlogIDs.IndexOf(b.BlogID));
            }
        }
    }

    public class TopicDisplay
    {
        public Topic topic { get; set; }
        public List<Blog> blogs { get; set; }
    }
}