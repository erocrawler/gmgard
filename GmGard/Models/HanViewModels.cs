using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GmGard.Models
{
    public class HanEdit
    {
        public int ID { get; set; }
        public string Uri { get; set; }

        [Required(ErrorMessage = "请输入标题"), StringLength(50, ErrorMessage = "标题不得超过50个字符"), Display(Name = "标题")]
        public string Title { get; set; }

        [Required(ErrorMessage = "请输入内容"), DataType(DataType.MultilineText), StringLength(2048, ErrorMessage = "简介不得超过2048字符")]
        public string Intro { get; set; }

        [StringLength(5452595, ErrorMessage = "封面不得超过4MB")]
        public string HanImage { get; set; }

        public string Logo { get; set; }

        public string Members { get; set; }

        public string Admins { get; set; }

        public string blogIDs { get; set; }

        public List<Blog> blogs { get; set; }

        public HanEdit(HanGroup hg)
        {
            ID = hg.HanGroupID;
            Uri = hg.GroupUri;
            Title = hg.Title;
            Intro = hg.Intro;
            Logo = hg.Logo;
            blogIDs = string.Join(",", hg.blogs.Select(h => h.BlogID));
            Members = string.Join(",", hg.members.Where(h => h.GroupLvl != 1).Select(h => h.Username));
            Admins = string.Join(",", hg.members.Where(h => h.GroupLvl == 1).Select(h => h.Username));
            blogs = hg.blogs.Select(h => h.blog).ToList();
        }

        public HanEdit()
        {
        }
    }

    public class HanDisplay
    {
        public HanGroup hangroup { get; set; }
        public List<BlogLink> grouplist { get; set; }
        public PagedList.IPagedList<Blog> blogs { get; set; }
    }
}